using OPTANO.Modeling.Common;
using OPTANO.Modeling.Optimization;
using OPTANO.Modeling.Optimization.Configuration;
using OPTANO.Modeling.Optimization.Enums;
using OPTANO.Modeling.Optimization.Solver.Highs14x;
using Wahlomat.config;
using Wahlomat.Config;
using Objective = OPTANO.Modeling.Optimization.Objective;

namespace Wahlomat.Optimization;

using Student = String;
using Project = String;
    
public class WahlomatModel
{
    private readonly RatingSystemOption _ratingSystem;
    private  Model _model;
    private readonly Config.Config _config;
    private readonly ObjectiveSense _objectiveSense;
    private readonly ProjectLimit _projectLimit;
    private readonly ResultCreator _resultCreator;
    public List<Student> Students { get; set; }
    public List<Project> Projects { get; set; }
    public Dictionary<(Student, Project), int> Rating { get; set; }
    
    public VariableCollection<Student, Project> Assignment { get; set; }
    
    public VariableCollection<string> WorstAssignment { get; set; }
    
    public VariableCollection<string> WorstProject { get; set; }
    
    public List<ProjectResult> Results { get; set; }
    
    public WahlomatModel(Config.Config config)
    {
        Students = new List<Student>();
        Projects = new List<Project>();
        Rating = new Dictionary<(Student, Project), int>();
        Results = new List<ProjectResult>();
        
        this._config = config;
        this._projectLimit = config.ProjectSizes["default"];
        this._objectiveSense =
            config.RatingSystem.System == RatingSystemOption.High ? ObjectiveSense.Maximize : ObjectiveSense.Minimize;
        this._ratingSystem = config.RatingSystem.System;
        _resultCreator = new ResultCreator(this);
    }
    
    public void Solve(string path)
    {
        FileHandler.Import(path, this);
        if (!this.Validate())
        {
            return;
        }
        
        var config = new Configuration
        {
            NameHandling = NameHandlingStyle.UniqueLongNames,
            ComputeRemovedVariables = true
        };
        using var scope = new ModelScope(config);
        this.CreateModel();

        using var solver = new HighsSolver14x();
        var solution = solver.Solve(this._model);

        // import the results back into the model 
        this._model.VariableCollections.ForEach(vc => vc.SetVariableValues(solution.VariableValues));
        _resultCreator.PrintSolution(path);
    }

    private bool Validate()
    {
        var evenDistribution = ((double)this.Students.Count) / this.Projects.Count;

        if (this._projectLimit.Minimum > evenDistribution)
        {
            Console.WriteLine($"Not enough students to reach the minimum of {this._projectLimit.Minimum} per project.");
            return false;
        }
        else if (this._projectLimit.Maximum < evenDistribution)
        {
            Console.WriteLine($"Not enough projects to stay under the maximum of {this._projectLimit.Maximum} per project.");
            return false;
        }

        return true;
    }

    public void CreateModel()
    {
        this._model = new Model();
        
        CreateVariables();
        CreateConstraints();
        if (this._config.Objectives.TotalRating.Active)
        {
            CreateTotalRatingObjective();
        }

        if (this._config.Objectives.WorstAssignment.Active)
        {
            CreateWorstAssignmentOptimization();
        }
    }

    private void CreateTotalRatingObjective()
    {
        var objective = new Objective(Expression.Sum(
            this.Students.SelectMany(s => 
                this.Projects.Select(p => this.Assignment[s,p] * this.Rating[(s,p)]))))
        {
            Sense = this._objectiveSense,
            PriorityLevel = this._config.Objectives.TotalRating.Priority,
            Weight = this._config.Objectives.TotalRating.Weight,
        };
        this._model.AddObjective(objective);
    }

    private void CreateWorstAssignmentOptimization()
    {
        this.WorstAssignment = new VariableCollection<Project>(
            this._model,
            this.Projects,
            "WorstAssignment",
            (p) => $"Worst assignment for project {p}",
            (p) => this._config.RatingSystem.MinValue,
            (p) => this._config.RatingSystem.MaxValue,
            (p) => VariableType.Continuous);

        this._model.AddConstraints(
            this.Projects.SelectMany(p =>
                this.Students.Select(s =>
                    this._objectiveSense == ObjectiveSense.Maximize ?
                    this.WorstAssignment[p] <=
                    this.Rating[(s, p)] * this.Assignment[s, p] :
                    this.WorstAssignment[p] >=
                    this.Rating[(s, p)] * this.Assignment[s, p] 
                    )));
        
        var objective = new Objective(Expression.Sum(
            this.Projects.Select(p => this.WorstAssignment[p])))
        {
            Sense = this._objectiveSense,
            PriorityLevel = this._config.Objectives.WorstAssignment.Priority,
            Weight = this._config.Objectives.WorstAssignment.Weight
        };
        this._model.AddObjective(objective);
    }

    private void CreateConstraints()
    {
        foreach (var s in Students)
        {
            // one project for each student
            this._model.AddConstraints(
                this.Students.Select(s => Expression.Sum(this.Projects.Select(p => this.Assignment[s, p])) == 1));
        }

        CreateSpecialAssignmentConstraints();
        CreateProjectLimitConstraints();
    }

    private void CreateProjectLimitConstraints()
    {
        foreach (var p in Projects)
        {
            var sum = Expression.Sum(this.Students.Select(s => this.Assignment[s, p]));
            this._model.AddConstraints(
                this.Projects.Select(p => GetProjectSize(p).Minimum <= sum));
            this._model.AddConstraints(
                this.Projects.Select(p => GetProjectSize(p).Maximum >= sum));
        }
    }

    private void CreateSpecialAssignmentConstraints()
    {
        if (this._config.Assignments == null)
        {
            return;
        }
        
        foreach (var (student,  projects) in this._config.Assignments.Forbid)
        {
            this._model.AddConstraints(projects.Select(p =>
                this.Assignment[student, p] == 0));    
        }
        foreach (var (student,  project) in this._config.Assignments.Enforce)
        {
            this._model.AddConstraint(this.Assignment[student, project] == 1);
        }
        foreach (var group in this._config.Assignments.Separate)
        {
            foreach (var project in Projects)
            {
                this._model.AddConstraint(Expression.Sum(group.Select(s => 
                    this.Assignment[s, project])) <= 1);
            }
        }
        foreach (var group in this._config.Assignments.Together)
        {
            for (var i = 0; i < group.Count - 1; i++)
            {
                for (var j = i + 1; i < group.Count; j++)
                {
                    this._model.AddConstraints(this.Projects.Select(p =>
                        this.Assignment[group[i], p] <= this.Assignment[group[j], p]));
                }
            } 
        }
    }

    private ProjectLimit GetProjectSize(string project)
    {
        return this._config.ProjectSizes.GetValueOrDefault(project, this._projectLimit);
    }

    private void CreateVariables()
    {
        this.Assignment = new VariableCollection<string, string>(
            this._model,
            this.Students,
            this.Projects,
            "Assignment",
            (s,p) => $"Student {s} gets project {p}",
            (s, p) => 0,
            (s, p) => 1,
            (s, p) => VariableType.Binary);
    }
}