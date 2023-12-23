using System.Runtime.CompilerServices;
using Wahlomat.config;

namespace Wahlomat.Config;

public class RatingSystem
{
    public int BestGrade { get; set; }
    public int WorstGrade { get; set; }

    public int MinValue => Math.Min(this.WorstGrade, this.BestGrade);

    public int MaxValue => Math.Max(this.WorstGrade, this.BestGrade);

    public RatingSystemOption System => BestGrade > WorstGrade ? 
        RatingSystemOption.High : 
        RatingSystemOption.Low;
}