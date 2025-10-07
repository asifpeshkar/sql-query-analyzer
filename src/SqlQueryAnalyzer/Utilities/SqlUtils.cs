using System.Text;
using System.Text.RegularExpressions;

namespace SqlQueryAnalyzer.Utilities;

public static class SqlUtils
{
    /// <summary>
    /// Normalizes SQL text by removing comments and extra whitespace
    /// </summary>
    public static string NormalizeSql(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
            return string.Empty;

        // Remove single-line comments (-- style)
        sql = Regex.Replace(sql, @"--.*$", "", RegexOptions.Multiline);
        
        // Remove multi-line comments (/* */ style)
        sql = Regex.Replace(sql, @"/\*.*?\*/", "", RegexOptions.Singleline);
        
        // Replace multiple whitespace with single space
        sql = Regex.Replace(sql, @"\s+", " ");
        
        // Trim and return
        return sql.Trim();
    }

    /// <summary>
    /// Splits SQL text into individual statements by GO (line) and semicolon separators
    /// </summary>
    public static List<string> SplitStatements(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
            return new List<string>();

        var statements = new List<string>();
        
        // First split by GO batch separator (must be on its own line)
        var batches = Regex.Split(sql, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        
        foreach (var batch in batches)
        {
            if (string.IsNullOrWhiteSpace(batch))
                continue;
                
            // Then split each batch by semicolons
            var batchStatements = batch.Split(';', StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var statement in batchStatements)
            {
                var trimmedStatement = statement.Trim();
                if (!string.IsNullOrWhiteSpace(trimmedStatement))
                {
                    statements.Add(trimmedStatement);
                }
            }
        }
        
        return statements;
    }

    /// <summary>
    /// Collapses whitespace outside of string literals, preserving string content and handling escaped quotes
    /// </summary>
    public static string CollapseWhitespaceOutsideStrings(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var result = new StringBuilder();
        var inSingleQuote = false;
        var inDoubleQuote = false;
        var lastWasWhitespace = false;
        
        for (int i = 0; i < input.Length; i++)
        {
            char current = input[i];
            char? next = i + 1 < input.Length ? input[i + 1] : null;
            
            // Handle single quotes
            if (current == '\'' && !inDoubleQuote)
            {
                // Check for escaped single quote
                if (next == '\'')
                {
                    // Double single quote (SQL escape)
                    result.Append(current);
                    result.Append(next.Value);
                    i++; // Skip the next character
                    lastWasWhitespace = false;
                    continue;
                }
                else
                {
                    inSingleQuote = !inSingleQuote;
                    result.Append(current);
                    lastWasWhitespace = false;
                    continue;
                }
            }
            
            // Handle double quotes
            if (current == '"' && !inSingleQuote)
            {
                // Check for escaped double quote
                if (next == '"')
                {
                    // Double double quote (SQL escape)
                    result.Append(current);
                    result.Append(next.Value);
                    i++; // Skip the next character
                    lastWasWhitespace = false;
                    continue;
                }
                else
                {
                    inDoubleQuote = !inDoubleQuote;
                    result.Append(current);
                    lastWasWhitespace = false;
                    continue;
                }
            }
            
            // Handle backslash escapes (for databases that support them)
            if (current == '\\' && (inSingleQuote || inDoubleQuote) && next != null)
            {
                // Preserve escaped character
                result.Append(current);
                result.Append(next.Value);
                i++; // Skip the next character
                lastWasWhitespace = false;
                continue;
            }
            
            // If we're inside a string literal, preserve all characters
            if (inSingleQuote || inDoubleQuote)
            {
                result.Append(current);
                lastWasWhitespace = false;
            }
            else
            {
                // Outside strings - handle whitespace collapse
                if (char.IsWhiteSpace(current))
                {
                    if (!lastWasWhitespace)
                    {
                        result.Append(' '); // Replace any whitespace with single space
                        lastWasWhitespace = true;
                    }
                }
                else
                {
                    result.Append(current);
                    lastWasWhitespace = false;
                }
            }
        }
        
        return result.ToString().Trim();
    }
}