using System.Text;
using System.Text.RegularExpressions;

namespace SqlQueryAnalyzer.Services;

public class SqlFormatter
{
    private readonly string[] _coreKeywords = {
        "SELECT", "FROM", "WHERE", "JOIN", "INNER", "LEFT", "RIGHT", "FULL", "OUTER", "CROSS",
        "GROUP", "BY", "ORDER", "HAVING", "UNION", "INSERT", "INTO", "UPDATE", "DELETE", "VALUES", 
        "ON", "SET", "AND", "OR"
    };

    private readonly string[] _majorClauses = {
        "SELECT", "FROM", "WHERE", "GROUP BY", "ORDER BY", "HAVING", "UNION", 
        "INSERT INTO", "UPDATE", "DELETE FROM", "VALUES", "SET"
    };

    private readonly string[] _joinKeywords = {
        "INNER JOIN", "LEFT JOIN", "RIGHT JOIN", "FULL JOIN", "OUTER JOIN", "CROSS JOIN", "JOIN"
    };

    public string Format(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
            return string.Empty;

        // Split by semicolons to handle multiple statements
        var statements = sql.Split(';', StringSplitOptions.RemoveEmptyEntries);
        var formattedStatements = new List<string>();

        foreach (var statement in statements)
        {
            var formatted = FormatSingleStatement(statement.Trim());
            if (!string.IsNullOrWhiteSpace(formatted))
            {
                formattedStatements.Add(formatted);
            }
        }

        return string.Join(";\n\n", formattedStatements) + (formattedStatements.Count > 0 ? ";" : "");
    }

    private string FormatSingleStatement(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
            return string.Empty;

        // Step 1: Preserve string literals and replace them with placeholders
        var (processedSql, stringLiterals) = PreserveStringLiterals(sql);
        
        // Step 2: Normalize whitespace (outside strings)
        processedSql = Regex.Replace(processedSql, @"\s+", " ").Trim();
        
        // Step 3: Uppercase core keywords
        processedSql = UppercaseKeywords(processedSql);
        
        // Step 4: Insert newlines before major clauses
        processedSql = InsertNewlinesBeforeClauses(processedSql);
        
        // Step 5: Put AND/OR on new lines
        processedSql = FormatAndOr(processedSql);
        
        // Step 6: Apply indentation
        processedSql = ApplyIndentation(processedSql);
        
        // Step 7: Restore string literals
        processedSql = RestoreStringLiterals(processedSql, stringLiterals);
        
        return processedSql;
    }

    private (string processedSql, List<string> stringLiterals) PreserveStringLiterals(string sql)
    {
        var stringLiterals = new List<string>();
        var result = new StringBuilder();
        var inSingleQuote = false;
        var inDoubleQuote = false;
        var currentString = new StringBuilder();
        
        for (int i = 0; i < sql.Length; i++)
        {
            char current = sql[i];
            char? next = i + 1 < sql.Length ? sql[i + 1] : null;
            
            if (current == '\'' && !inDoubleQuote)
            {
                if (inSingleQuote)
                {
                    // Check for escaped quote
                    if (next == '\'')
                    {
                        currentString.Append(current);
                        currentString.Append(next.Value);
                        i++; // Skip next character
                        continue;
                    }
                    else
                    {
                        // End of string
                        currentString.Append(current);
                        stringLiterals.Add(currentString.ToString());
                        result.Append($"__STRING_{stringLiterals.Count - 1}__");
                        currentString.Clear();
                        inSingleQuote = false;
                    }
                }
                else
                {
                    // Start of string
                    currentString.Append(current);
                    inSingleQuote = true;
                }
            }
            else if (current == '"' && !inSingleQuote)
            {
                if (inDoubleQuote)
                {
                    // Check for escaped quote
                    if (next == '"')
                    {
                        currentString.Append(current);
                        currentString.Append(next.Value);
                        i++; // Skip next character
                        continue;
                    }
                    else
                    {
                        // End of string
                        currentString.Append(current);
                        stringLiterals.Add(currentString.ToString());
                        result.Append($"__STRING_{stringLiterals.Count - 1}__");
                        currentString.Clear();
                        inDoubleQuote = false;
                    }
                }
                else
                {
                    // Start of string
                    currentString.Append(current);
                    inDoubleQuote = true;
                }
            }
            else if (inSingleQuote || inDoubleQuote)
            {
                currentString.Append(current);
            }
            else
            {
                result.Append(current);
            }
        }
        
        return (result.ToString(), stringLiterals);
    }

    private string UppercaseKeywords(string sql)
    {
        foreach (var keyword in _coreKeywords)
        {
            var pattern = $@"\b{Regex.Escape(keyword)}\b";
            sql = Regex.Replace(sql, pattern, keyword.ToUpper(), RegexOptions.IgnoreCase);
        }
        return sql;
    }

    private string InsertNewlinesBeforeClauses(string sql)
    {
        // Handle compound keywords first (GROUP BY, ORDER BY, etc.)
        sql = Regex.Replace(sql, @"\b(GROUP BY|ORDER BY|INSERT INTO|DELETE FROM)\b", "\n$1", RegexOptions.IgnoreCase);
        
        // Handle JOIN variants (keep them together)
        sql = Regex.Replace(sql, @"\b(INNER JOIN|LEFT JOIN|RIGHT JOIN|FULL JOIN|OUTER JOIN|CROSS JOIN|JOIN)\b", "\n$1", RegexOptions.IgnoreCase);
        
        // Handle other single keywords
        sql = Regex.Replace(sql, @"\b(FROM|WHERE|HAVING|UNION|UPDATE|VALUES|SET)\b", "\n$1", RegexOptions.IgnoreCase);
        
        // Clean up leading newlines
        sql = sql.TrimStart('\n');
        
        return sql;
    }

    private string FormatAndOr(string sql)
    {
        // Put AND/OR on new lines within conditions
        sql = Regex.Replace(sql, @"\s+(AND|OR)\s+", "\n$1 ", RegexOptions.IgnoreCase);
        return sql;
    }

    private string ApplyIndentation(string sql)
    {
        var lines = sql.Split('\n');
        var result = new List<string>();
        int indentLevel = 0;
        
        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmedLine))
                continue;
                
            // Handle SELECT column formatting
            if (Regex.IsMatch(trimmedLine, @"^SELECT\b", RegexOptions.IgnoreCase))
            {
                var formattedSelect = FormatSelectClause(trimmedLine);
                foreach (var selectLine in formattedSelect)
                {
                    string selectIndent = new string(' ', indentLevel * 4);
                    result.Add(selectIndent + selectLine);
                }
                indentLevel++;
                continue;
            }
                
            // Decrease indent for closing parentheses or specific keywords
            if (trimmedLine.StartsWith(")") || 
                Regex.IsMatch(trimmedLine, @"^(ON|WHERE|HAVING)\b", RegexOptions.IgnoreCase))
            {
                indentLevel = Math.Max(0, indentLevel - 1);
            }
            
            // Apply indentation
            string lineIndent = new string(' ', indentLevel * 4);
            result.Add(lineIndent + trimmedLine);
            
            // Increase indent after major clauses or opening parentheses
            if (Regex.IsMatch(trimmedLine, @"^(FROM|WHERE|GROUP BY|ORDER BY|HAVING|UNION|VALUES|SET)\b", RegexOptions.IgnoreCase) ||
                trimmedLine.EndsWith("("))
            {
                indentLevel++;
            }
        }
        
        return string.Join("\n", result);
    }

    private List<string> FormatSelectClause(string selectLine)
    {
        var result = new List<string>();
        
        // Extract the SELECT keyword and any modifiers (DISTINCT, TOP, etc.)
        var selectMatch = Regex.Match(selectLine, @"^(SELECT\s+(?:DISTINCT\s+|TOP\s+\d+\s+|TOP\s*\(\s*\d+\s*\)\s+)*)", RegexOptions.IgnoreCase);
        if (!selectMatch.Success)
        {
            result.Add(selectLine);
            return result;
        }

        string selectKeyword = selectMatch.Groups[1].Value.Trim();
        string columnsPart = selectLine.Substring(selectMatch.Length).Trim();
        
        if (string.IsNullOrWhiteSpace(columnsPart))
        {
            result.Add(selectKeyword);
            return result;
        }

        // Split columns respecting strings and parentheses
        var columns = SplitColumnsRespectingDelimiters(columnsPart);
        
        if (columns.Count == 0)
        {
            result.Add(selectLine);
            return result;
        }

        // Add SELECT keyword
        result.Add(selectKeyword);
        
        // Add each column on its own line with 2-space indent
        for (int i = 0; i < columns.Count; i++)
        {
            string column = columns[i].Trim();
            if (string.IsNullOrWhiteSpace(column))
                continue;
                
            // Add comma prefix for all but the first column
            string prefix = i == 0 ? "  " : ", ";
            result.Add(prefix + column);
        }
        
        return result;
    }

    private List<string> SplitColumnsRespectingDelimiters(string columnsPart)
    {
        var columns = new List<string>();
        var currentColumn = new StringBuilder();
        int parenDepth = 0;
        bool inSingleQuote = false;
        bool inDoubleQuote = false;
        
        for (int i = 0; i < columnsPart.Length; i++)
        {
            char current = columnsPart[i];
            char? next = i + 1 < columnsPart.Length ? columnsPart[i + 1] : null;
            
            // Handle single quotes
            if (current == '\'' && !inDoubleQuote)
            {
                currentColumn.Append(current);
                if (inSingleQuote && next == '\'')
                {
                    // Escaped single quote
                    currentColumn.Append(next.Value);
                    i++; // Skip next character
                }
                else
                {
                    inSingleQuote = !inSingleQuote;
                }
                continue;
            }
            
            // Handle double quotes
            if (current == '"' && !inSingleQuote)
            {
                currentColumn.Append(current);
                if (inDoubleQuote && next == '"')
                {
                    // Escaped double quote
                    currentColumn.Append(next.Value);
                    i++; // Skip next character
                }
                else
                {
                    inDoubleQuote = !inDoubleQuote;
                }
                continue;
            }
            
            // If we're inside quotes, just append
            if (inSingleQuote || inDoubleQuote)
            {
                currentColumn.Append(current);
                continue;
            }
            
            // Handle parentheses
            if (current == '(')
            {
                parenDepth++;
                currentColumn.Append(current);
            }
            else if (current == ')')
            {
                parenDepth--;
                currentColumn.Append(current);
            }
            else if (current == ',' && parenDepth == 0)
            {
                // This is a column separator
                columns.Add(currentColumn.ToString());
                currentColumn.Clear();
            }
            else
            {
                currentColumn.Append(current);
            }
        }
        
        // Add the last column
        if (currentColumn.Length > 0)
        {
            columns.Add(currentColumn.ToString());
        }
        
        return columns;
    }

    private string RestoreStringLiterals(string sql, List<string> stringLiterals)
    {
        for (int i = 0; i < stringLiterals.Count; i++)
        {
            sql = sql.Replace($"__STRING_{i}__", stringLiterals[i]);
        }
        return sql;
    }
}