namespace SqlQueryAnalyzer.Models;

public enum IssueSeverity
{
    Info,
    Warning,
    Error
}

public class Span
{
    public int Start { get; set; }
    public int Length { get; set; }
    
    public Span(int start, int length)
    {
        Start = start;
        Length = length;
    }
}

public class Issue
{
    public string RuleId { get; set; } = string.Empty;
    public IssueSeverity Severity { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Suggestion { get; set; }
    public int? StatementIndex { get; set; }
    public Span? Span { get; set; }
}