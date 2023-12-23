using System.Collections;
using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Stupro.Optimization;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Stupro;

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
    
    public static bool Import(string path, StuproModel stuproModel)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";", 
            Encoding = Encoding.UTF8,
        };
        
        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, config);
        csv.Read();
        var header = csv.ReadHeader();
        if (!header)
        {
            Console.WriteLine("Header missing in {path}");
            return false;
        }
        for (var i = 1; i < csv.HeaderRecord!.Length; i++)
        {
            stuproModel.Projects.Add(csv.HeaderRecord[i]);
        }

        var line = 2;
        while (csv.Read())
        {
            var student = csv.GetField("Name");
            if (student == null)
            {
                Console.WriteLine($"Error reading {path}: No student name in line {line}!");
                return false;
            }
            stuproModel.Students.Add(student);
            foreach (var project in stuproModel.Projects)
            {
                stuproModel.Rating[(student, project)] = csv.GetField<int>(project);
            }

            line++;
        }

        return true;
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

    public static void WriteCsvFile(string outputPath, IEnumerable records)
    {
        var culture = new CultureInfo("");
        culture.NumberFormat.NumberDecimalDigits = 2;
        using (var writer = new StreamWriter(outputPath))
        using (var csv = new CsvWriter(writer, culture))
        {
            csv.WriteRecords(records);
        }
    }
}