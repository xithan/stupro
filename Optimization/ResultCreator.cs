using System.Collections;
using System.Globalization;
using CsvHelper;

namespace Stupro.Optimization;

public class ResultCreator
{
    private StuproModel _stuproModel;

    public ResultCreator(StuproModel stuproModel)
    {
        _stuproModel = stuproModel;
    }

    public void PrintSolution(string inputPath)
    {
        foreach (var student in _stuproModel.Students)
        {
            foreach (var project in _stuproModel.Projects.Where(project => _stuproModel.Assignment[student, project].Value > 0))
            {
                Console.WriteLine($"{student}: {project} ({_stuproModel.Rating[(student, project)]})");
            }
        }
        
        foreach (var project in _stuproModel.Projects)
        {
            var participants = 
                _stuproModel.Students.Where(s => _stuproModel.Assignment[s, project].Value > 0)
                    .ToList();
            var count = participants.Count();
            var ratings = participants.Select(s => _stuproModel.Rating[(s, project)]).ToList();
            var totalRating = ratings.Sum();
            Console.WriteLine($"{project}: {count} Students, Total rating: {totalRating}");
            _stuproModel.Results.Add(new ProjectResult()
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
        
        var resultPath = $"{withoutExtension}_result.csv";
        FileHandler.WriteCsvFile(resultPath, _stuproModel.Results);
        
        resultPath = $"{withoutExtension}_assignments.csv";
        using var pwriter = new StreamWriter(resultPath);
        foreach(var result in _stuproModel.Results) {
            pwriter.Write(result.Project);
            foreach (var student in result.Students)
            {
                pwriter.Write($";{student} ({_stuproModel.Rating[(student, result.Project)]})");
            }
            pwriter.WriteLine();
        }
    }
}