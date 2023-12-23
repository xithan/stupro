using Mono.Options;

namespace Stupro;

public class ProgramArguments
{
    public bool ShowHelp { get; set; }
    public string ConfigPath { get; set; }= "config.yml";
    public bool Generate { get; set; }
    public int ProjectCount { get; set; } = 30;
    public int StudentCount { get; set; } = 812;
    
    public string Path { get; set; }

    public bool Parse(string[] args)
    {
        var p = new OptionSet () {
            "Usage: stupro [OPTIONS]+ path",
            "Reads a csv file with student names and ratings for projects.\n" +
            "Assigns to each student a project to maximize or minimize the ratings.\n" +
            "Writes the result to <path>_result.csv and <path>_assignments.csv." +
            "The tool can also be used to generate test data.",
            "",
            "Options:",
            { "g|generate=", "generate test data and save it to the given path",
                (bool g) => Generate = g
            },
            { "students=", "the number of student to generate",
                (int v) => StudentCount = v
            },
            { "projects=", "the number of projects to generate",
                (int v) => ProjectCount = v
            },
            { "config=", 
                "The path to the config file. See example_config.yml for details. Default is config.yml",
                v => ConfigPath = v },
            { "h|help",  "show this message and exit", 
                v => ShowHelp = v != null },
        };
        
        List<string> extra;
        try {
            extra = p.Parse (args);
        }
        catch (OptionException e) {
            Console.Write ("stupro: ");
            Console.WriteLine (e.Message);
            Console.WriteLine ("Try `stupro --help' for more information.");
            return false;
        }

        if (ShowHelp) {
            p.WriteOptionDescriptions (Console.Out);
            return false;
        }

        if (extra.Count == 0)
        {
            Console.WriteLine("No path given");
            p.WriteOptionDescriptions (Console.Out);
            return false;
        }

        this.Path = extra[0];
        return true;
    }
}