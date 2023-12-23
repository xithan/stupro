// ReSharper disable CollectionNeverUpdated.Global
namespace Stupro.Config;

public class Config  
{
    public Objectives Objectives { get; set; }
    
    public Dictionary<string, ProjectLimit> ProjectSizes { get; set; }
    
    public RatingSystem RatingSystem { get; set; }
    
    public Assignments? Assignments { get; set; }
}