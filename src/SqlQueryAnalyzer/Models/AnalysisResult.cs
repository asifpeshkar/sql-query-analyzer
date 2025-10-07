namespace SqlQueryAnalyzer.Models;

public class AnalysisResult
{
    public List<Issue> Issues { get; set; } = new();
    public bool HasErrors => Issues.Any(issue => issue.Severity == IssueSeverity.Error);
}