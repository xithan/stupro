using Stupro;
using Stupro.Optimization;

var arguments = new ProgramArguments();
if (!arguments.Parse(args))
{
    return;
}

if (arguments.Generate)
{
    FileHandler.CreateFile(arguments.Path, arguments.StudentCount, arguments.ProjectCount);
    Console.WriteLine($"Generated test data with {arguments.StudentCount} students and " +
                      $"{arguments.ProjectCount} projects stored in {arguments.Path}.");
    return;
}

var config = FileHandler.ReadConfigFile(arguments.ConfigPath);
var model = new StuproModel(config);
model.Solve(arguments.Path);


