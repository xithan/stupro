// ReSharper disable CollectionNeverUpdated.Global
namespace Stupro.Config;

public class Assignments
{
    public Assignments()
    {
        Forbid = new Dictionary<string, List<string>>();
        Enforce = new Dictionary<string, string>();
        Separate = new List<List<string>>();
        Together = new List<List<string>>();
    }
    
    public Dictionary<string, List<string>> Forbid { get; set; }
    public Dictionary<string, string> Enforce { get; set; }
    public List<List<string>> Separate { get; set; }
    public List<List<string>> Together { get; set; }
}
