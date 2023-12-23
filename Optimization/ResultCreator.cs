using System.Collections;
using System.Globalization;
using CsvHelper;

namespace Wahlomat.Optimization;

public class ResultCreator
{
    private WahlomatModel _wahlomatModel;

    public ResultCreator(WahlomatModel wahlomatModel)
    {
        _wahlomatModel = wahlomatModel;
    }

    public void PrintSolution(string inputPath)
    {
        foreach (var student in _wahlomatModel.Students)
        {
            foreach (var project in _wahlomatModel.Projects.Where(project => _wahlomatModel.Assignment[student, project].Value > 0))
            {
                Console.WriteLine($"{student}: {project} ({_wahlomatModel.Rating[(student, project)]})");
            }
        }
        
        foreach (var project in _wahlomatModel.Projects)
        {
            var participants = 
                _wahlomatModel.Students.Where(s => _wahlomatModel.Assignment[s, project].Value > 0)
                    .ToList();
            var count = participants.Count();
            var ratings = participants.Select(s => _wahlomatModel.Rating[(s, project)]).ToList();
            var totalRating = ratings.Sum();
            Console.WriteLine($"{project}: {count} Students, Total rating: {totalRating}");
            _wahlomatModel.Results.Add(new ProjectResult()
            {
                Project = project,
                Students = participants,
                TotalRating = totalRating,
                MinRating = ratings.Min(),
                MaxRating = ratings.Max(),
                AverageRating = ratings.Average(),
            });
        }
        this.WriteResult(inputPath);
    }

    public void WriteResult(string inputPath)
    {
        var withoutExtension = Path.Join(Path.GetDirectoryName(inputPath),
            Path.GetFileNameWithoutExtension(inputPath));
        var outputPath = $"{withoutExtension}_result.csv";
        using (var writer = new StreamWriter(outputPath))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.WriteRecords((IEnumerable)_wahlomatModel.Results);
        }
        outputPath = $"{withoutExtension}_assignments.csv";
        using (var pwriter = new StreamWriter(outputPath))
            foreach(var result in _wahlomatModel.Results) {
                pwriter.Write(result.Project);
                foreach (var student in result.Students)
                {
                    pwriter.Write($";{student} ({_wahlomatModel.Rating[(student, result.Project)]})");
                }
                pwriter.WriteLine();
            }
    }
}