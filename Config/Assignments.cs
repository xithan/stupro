namespace Wahlomat.Config;

public class Assignments
{
    public Dictionary<string, List<string>> Forbid { get; set; }
    public Dictionary<string, string> Enforce { get; set; }
    public List<List<string>> Separate { get; set; }
    public List<List<string>> Together { get; set; }
}
