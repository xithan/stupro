namespace Stupro.Optimization;

public class ProjectResult
{
    public string Project { get; set; }
    public List<string> Students { get; set; }
    public int TotalRating { get; set; }
    public int MinRating { get; set; }
    public int MaxRating { get; set; }
    
    public double AverageRating { get; set; }
}