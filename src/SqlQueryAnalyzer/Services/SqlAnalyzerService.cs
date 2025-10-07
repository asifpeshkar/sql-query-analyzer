using SqlQueryAnalyzer.Models;
using SqlQueryAnalyzer.Utilities;
using System.Text.RegularExpressions;

namespace SqlQueryAnalyzer.Services;

public class SqlAnalyzerService
{
    public AnalysisResult Analyze(string sqlContent)
    {
        var result = new AnalysisResult();
        
        // Clean and normalize the SQL
        string normalizedSql = SqlUtils.NormalizeSql(sqlContent);
        
        // Split into individual statements
        var statements = SqlUtils.SplitStatements(normalizedSql);
        
        // Run checkers on each statement
        for (int i = 0; i < statements.Count; i++)
        {
            var statement = statements[i];
            var statementIndex = i;
            
            result.Issues.AddRange(CheckSQL001_SelectStar(statement, statementIndex));
            result.Issues.AddRange(CheckSQL010_DestructiveStatements(statement, statementIndex));
            result.Issues.AddRange(CheckSQL020_DeleteWithoutWhere(statement, statementIndex));
            result.Issues.AddRange(CheckSQL021_UpdateWithoutWhere(statement, statementIndex));
            result.Issues.AddRange(CheckSQL030_ExplicitCrossJoin(statement, statementIndex));
            result.Issues.AddRange(CheckSQL031_ImplicitCommaJoins(statement, statementIndex));
            result.Issues.AddRange(CheckSQL040_NolockHints(statement, statementIndex));
            result.Issues.AddRange(CheckSQL041_TopWithoutOrderBy(statement, statementIndex));
            result.Issues.AddRange(CheckSQL042_SelectIntoWithoutColumns(statement, statementIndex));
            result.Issues.AddRange(CheckSQL043_JoinMissingOn(statement, statementIndex));
            result.Issues.AddRange(CheckSQL044_CartesianRisk(statement, statementIndex));
        }
        
        return result;
    }

    /// <summary>
    /// SQL001: Detect SELECT * usage
    /// </summary>
    private List<Issue> CheckSQL001_SelectStar(string statement, int statementIndex)
    {
        var issues = new List<Issue>();
        
        if (Regex.IsMatch(statement, @"\bSELECT\s+\*", RegexOptions.IgnoreCase))
        {
            issues.Add(new Issue
            {
                RuleId = "SQL001",
                Message = "SELECT * should be avoided",
                Severity = IssueSeverity.Warning,
                Suggestion = "Specify explicit column names",
                StatementIndex = statementIndex
            });
        }
        
        return issues;
    }

    /// <summary>
    /// SQL010: Detect destructive statements at start (DROP/TRUNCATE/ALTER)
    /// </summary>
    private List<Issue> CheckSQL010_DestructiveStatements(string statement, int statementIndex)
    {
        var issues = new List<Issue>();
        var trimmedStatement = statement.Trim();
        
        // Check for DROP statements
        if (Regex.IsMatch(trimmedStatement, @"^\s*DROP\s+(TABLE|DATABASE|SCHEMA|INDEX|VIEW|PROCEDURE|FUNCTION)", RegexOptions.IgnoreCase))
        {
            issues.Add(new Issue
            {
                RuleId = "SQL010",
                Message = "Destructive DROP statement detected",
                Severity = IssueSeverity.Error,
                Suggestion = "Ensure backup exists before dropping",
                StatementIndex = statementIndex
            });
        }
        
        // Check for TRUNCATE statements
        if (Regex.IsMatch(trimmedStatement, @"^\s*TRUNCATE\s+TABLE\b", RegexOptions.IgnoreCase))
        {
            issues.Add(new Issue
            {
                RuleId = "SQL010", 
                Message = "Destructive TRUNCATE statement detected",
                Severity = IssueSeverity.Error,
                Suggestion = "TRUNCATE cannot be rolled back",
                StatementIndex = statementIndex
            });
        }
        
        // Check for potentially destructive ALTER statements
        if (Regex.IsMatch(trimmedStatement, @"^\s*ALTER\s+TABLE\s+.*\bDROP\s+(COLUMN|CONSTRAINT)", RegexOptions.IgnoreCase))
        {
            issues.Add(new Issue
            {
                RuleId = "SQL010",
                Message = "Destructive ALTER TABLE statement detected",
                Severity = IssueSeverity.Warning,
                Suggestion = "Dropping columns may cause data loss",
                StatementIndex = statementIndex
            });
        }
        
        return issues;
    }

    /// <summary>
    /// SQL020: Detect DELETE without WHERE clause
    /// </summary>
    private List<Issue> CheckSQL020_DeleteWithoutWhere(string statement, int statementIndex)
    {
        var issues = new List<Issue>();
        var trimmedStatement = statement.Trim();
        
        if (Regex.IsMatch(trimmedStatement, @"^\s*DELETE\s+FROM\s+\w+", RegexOptions.IgnoreCase) &&
            !Regex.IsMatch(trimmedStatement, @"\bWHERE\b", RegexOptions.IgnoreCase))
        {
            issues.Add(new Issue
            {
                RuleId = "SQL020",
                Message = "DELETE without WHERE clause",
                Severity = IssueSeverity.Error,
                Suggestion = "Add WHERE clause to limit scope",
                StatementIndex = statementIndex
            });
        }
        
        return issues;
    }

    /// <summary>
    /// SQL021: Detect UPDATE without WHERE clause
    /// </summary>
    private List<Issue> CheckSQL021_UpdateWithoutWhere(string statement, int statementIndex)
    {
        var issues = new List<Issue>();
        var trimmedStatement = statement.Trim();
        
        if (Regex.IsMatch(trimmedStatement, @"^\s*UPDATE\s+\w+\s+SET\s+", RegexOptions.IgnoreCase) &&
            !Regex.IsMatch(trimmedStatement, @"\bWHERE\b", RegexOptions.IgnoreCase))
        {
            issues.Add(new Issue
            {
                RuleId = "SQL021",
                Message = "UPDATE without WHERE clause",
                Severity = IssueSeverity.Error,
                Suggestion = "Add WHERE clause to limit scope",
                StatementIndex = statementIndex
            });
        }
        
        return issues;
    }

    /// <summary>
    /// SQL030: Detect explicit CROSS JOIN
    /// </summary>
    private List<Issue> CheckSQL030_ExplicitCrossJoin(string statement, int statementIndex)
    {
        var issues = new List<Issue>();
        
        if (Regex.IsMatch(statement, @"\bCROSS\s+JOIN\b", RegexOptions.IgnoreCase))
        {
            issues.Add(new Issue
            {
                RuleId = "SQL030",
                Message = "CROSS JOIN detected",
                Severity = IssueSeverity.Warning,
                Suggestion = "Verify cartesian product is intended",
                StatementIndex = statementIndex
            });
        }
        
        return issues;
    }

    /// <summary>
    /// SQL031: Detect implicit comma joins in FROM clause
    /// </summary>
    private List<Issue> CheckSQL031_ImplicitCommaJoins(string statement, int statementIndex)
    {
        var issues = new List<Issue>();
        
        // Look for comma-separated tables in FROM clause without explicit JOIN
        var fromMatch = Regex.Match(statement, @"\bFROM\s+(.*?)(?:\bWHERE\b|\bGROUP\b|\bORDER\b|\bHAVING\b|$)", 
            RegexOptions.IgnoreCase | RegexOptions.Singleline);
        
        if (fromMatch.Success)
        {
            string fromClause = fromMatch.Groups[1].Value;
            // Check if there are commas but no explicit JOINs in the FROM clause
            if (fromClause.Contains(',') && !Regex.IsMatch(fromClause, @"\bJOIN\b", RegexOptions.IgnoreCase))
            {
                issues.Add(new Issue
                {
                    RuleId = "SQL031",
                    Message = "Implicit comma join detected",
                    Severity = IssueSeverity.Warning,
                    Suggestion = "Use explicit JOIN syntax",
                    StatementIndex = statementIndex
                });
            }
        }
        
        return issues;
    }

    /// <summary>
    /// SQL040: Detect NOLOCK hints
    /// </summary>
    private List<Issue> CheckSQL040_NolockHints(string statement, int statementIndex)
    {
        var issues = new List<Issue>();
        
        if (Regex.IsMatch(statement, @"\bWITH\s*\(\s*NOLOCK\s*\)", RegexOptions.IgnoreCase) ||
            Regex.IsMatch(statement, @"\(NOLOCK\)", RegexOptions.IgnoreCase))
        {
            issues.Add(new Issue
            {
                RuleId = "SQL040",
                Message = "NOLOCK hint detected",
                Severity = IssueSeverity.Warning,
                Suggestion = "NOLOCK may cause dirty reads",
                StatementIndex = statementIndex
            });
        }
        
        return issues;
    }

    /// <summary>
    /// SQL041: Detect TOP without ORDER BY
    /// </summary>
    private List<Issue> CheckSQL041_TopWithoutOrderBy(string statement, int statementIndex)
    {
        var issues = new List<Issue>();
        
        if (Regex.IsMatch(statement, @"\bTOP\s+\d+", RegexOptions.IgnoreCase) ||
            Regex.IsMatch(statement, @"\bTOP\s*\(\s*\d+\s*\)", RegexOptions.IgnoreCase))
        {
            if (!Regex.IsMatch(statement, @"\bORDER\s+BY\b", RegexOptions.IgnoreCase))
            {
                issues.Add(new Issue
                {
                    RuleId = "SQL041",
                    Message = "TOP without ORDER BY",
                    Severity = IssueSeverity.Warning,
                    Suggestion = "Add ORDER BY for consistent results",
                    StatementIndex = statementIndex
                });
            }
        }
        
        return issues;
    }

    /// <summary>
    /// SQL042: Detect SELECT INTO without explicit column list
    /// </summary>
    private List<Issue> CheckSQL042_SelectIntoWithoutColumns(string statement, int statementIndex)
    {
        var issues = new List<Issue>();
        
        if (Regex.IsMatch(statement, @"\bSELECT\s+\*.*\bINTO\b", RegexOptions.IgnoreCase | RegexOptions.Singleline))
        {
            issues.Add(new Issue
            {
                RuleId = "SQL042",
                Message = "SELECT INTO without explicit column list",
                Severity = IssueSeverity.Warning,
                Suggestion = "Specify explicit columns in SELECT",
                StatementIndex = statementIndex
            });
        }
        
        return issues;
    }

    /// <summary>
    /// SQL043: Detect JOIN missing ON clause (heuristic)
    /// </summary>
    private List<Issue> CheckSQL043_JoinMissingOn(string statement, int statementIndex)
    {
        var issues = new List<Issue>();
        
        // Look for JOIN keyword followed by a table name but no ON before next major clause
        // Exclude CROSS JOIN as it doesn't require ON clause
        var joinMatches = Regex.Matches(statement, @"\b(INNER\s+JOIN|LEFT\s+JOIN|RIGHT\s+JOIN|FULL\s+JOIN|(?<!CROSS\s+)JOIN)\s+\w+", RegexOptions.IgnoreCase);
        
        foreach (Match joinMatch in joinMatches)
        {
            int joinPosition = joinMatch.Index + joinMatch.Length;
            string remainingStatement = statement.Substring(joinPosition);
            
            // Check if there's an ON clause before the next major clause
            var nextClauseMatch = Regex.Match(remainingStatement, @"\b(WHERE|GROUP\s+BY|ORDER\s+BY|HAVING|UNION|JOIN|INNER\s+JOIN|LEFT\s+JOIN|RIGHT\s+JOIN|FULL\s+JOIN)\b", RegexOptions.IgnoreCase);
            
            string beforeNextClause = nextClauseMatch.Success ? remainingStatement.Substring(0, nextClauseMatch.Index) : remainingStatement;
            
            if (!Regex.IsMatch(beforeNextClause, @"\bON\b", RegexOptions.IgnoreCase))
            {
                issues.Add(new Issue
                {
                    RuleId = "SQL043",
                    Message = "JOIN missing ON clause",
                    Severity = IssueSeverity.Error,
                    Suggestion = "Add ON clause to specify join condition",
                    StatementIndex = statementIndex
                });
                break; // Only report once per statement
            }
        }
        
        return issues;
    }

    /// <summary>
    /// SQL044: Detect Cartesian risk - multiple tables in FROM with no WHERE/ON
    /// </summary>
    private List<Issue> CheckSQL044_CartesianRisk(string statement, int statementIndex)
    {
        var issues = new List<Issue>();
        
        // Look for FROM clause with multiple tables
        var fromMatch = Regex.Match(statement, @"\bFROM\s+(.*?)(?:\bWHERE\b|\bGROUP\b|\bORDER\b|\bHAVING\b|$)", 
            RegexOptions.IgnoreCase | RegexOptions.Singleline);
        
        if (fromMatch.Success)
        {
            string fromClause = fromMatch.Groups[1].Value;
            
            // Count table references (comma-separated or multiple table names without JOIN)
            bool hasMultipleTables = fromClause.Contains(',') || 
                                   (Regex.Matches(fromClause, @"\b\w+\s+\w+", RegexOptions.IgnoreCase).Count > 1 && 
                                    !Regex.IsMatch(fromClause, @"\bJOIN\b", RegexOptions.IgnoreCase));
            
            if (hasMultipleTables)
            {
                // Check if there's any WHERE clause or ON clause in the statement
                bool hasWhereOrOn = Regex.IsMatch(statement, @"\b(WHERE|ON)\b", RegexOptions.IgnoreCase);
                
                if (!hasWhereOrOn)
                {
                    issues.Add(new Issue
                    {
                        RuleId = "SQL044",
                        Message = "Cartesian product risk detected",
                        Severity = IssueSeverity.Warning,
                        Suggestion = "Add WHERE or JOIN conditions",
                        StatementIndex = statementIndex
                    });
                }
            }
        }
        
        return issues;
    }
}