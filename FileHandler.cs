using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Wahlomat.Optimization;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Wahlomat;

public class FileHandler
{
    public static void CreateFile(string path, int studentCount, int projectCount)
    {
        using StreamWriter outputFile = new StreamWriter(path);
        Random rnd = new Random();
           
        outputFile.Write("Name");
        for (var i = 1; i <= projectCount; i++)
        {
            outputFile.Write($",P{i}");
        }
        outputFile.WriteLine();
        for (var s = 1; s < studentCount; s++)
        {
            outputFile.Write($"S{s}");
            for (var p = 1; p <= projectCount; p++)
            {
                outputFile.Write($",{rnd.Next(1, 7)}");
            }
            outputFile.WriteLine();
        }
    }
    
    public static void Import(string path, WahlomatModel wahlomatModel)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";", Encoding = Encoding.UTF8 };
        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, config);
        csv.Read();
        var header = csv.ReadHeader();
        for (var i = 1; i < csv.HeaderRecord.Length; i++)
        {
            wahlomatModel.Projects.Add(csv.HeaderRecord[i]);
        }
        while (csv.Read())
        {
            var student = csv.GetField("Name");
            wahlomatModel.Students.Add(student);
            foreach (var project in wahlomatModel.Projects)
            {
                wahlomatModel.Rating[(student, project)] = csv.GetField<int>(project);
            }
        }
    }
    
    public static Config.Config ReadConfigFile(string configPath)
    {
        var deserializer = new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();
        var yml = File.ReadAllText(configPath);   
        var config = deserializer.Deserialize<Config.Config>(yml);
        return config;
    }
}