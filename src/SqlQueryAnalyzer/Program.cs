using SqlQueryAnalyzer.Services;

namespace SqlQueryAnalyzer;

class Program
{
    static void Main(string[] args)
    {        
        try
        {
            string sqlContent = GetSqlInput(args);
            
            if (string.IsNullOrWhiteSpace(sqlContent))
            {
                ShowUsage();
                return;
            }

            var analyzer = new SqlAnalyzerService();
            var formatter = new SqlFormatter();

            // Analyze the SQL
            var analysisResult = analyzer.Analyze(sqlContent);

            // Display Analysis Results
            Console.WriteLine("Analysis Results:");
            Console.WriteLine("================");
            if (analysisResult.Issues.Any())
            {
                foreach (var issue in analysisResult.Issues)
                {
                    Console.WriteLine($"RuleId: {issue.RuleId}");
                    Console.WriteLine($"Severity: {issue.Severity}");
                    Console.WriteLine($"Message: {issue.Message}");
                    if (!string.IsNullOrEmpty(issue.Suggestion))
                    {
                        Console.WriteLine($"Suggestion: {issue.Suggestion}");
                    }
                    if (issue.StatementIndex.HasValue)
                    {
                        Console.WriteLine($"StatementIndex: {issue.StatementIndex}");
                    }
                    if (issue.Span != null)
                    {
                        Console.WriteLine($"Span: Start={issue.Span.Start}, Length={issue.Span.Length}");
                    }
                    Console.WriteLine();
                }
            }
            else
            {
                Console.WriteLine("No issues found.");
                Console.WriteLine();
            }

            // Display Beautified SQL
            Console.WriteLine("Beautified SQL:");
            Console.WriteLine("===============");
            string formattedSql = formatter.Format(sqlContent);
            Console.WriteLine(formattedSql);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Environment.Exit(1);
        }
    }

    private static string GetSqlInput(string[] args)
    {
        // Parse command line arguments
        for (int i = 0; i < args.Length; i++)
        {
            if ((args[i] == "-f" || args[i] == "--file") && i + 1 < args.Length)
            {
                string filePath = args[i + 1];
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"File not found: {filePath}");
                }
                return File.ReadAllText(filePath);
            }
        }

        // Check if input is redirected (piped)
        if (Console.IsInputRedirected)
        {
            return Console.In.ReadToEnd();
        }

        // Interactive paste mode
        return ReadFromConsoleUntilEOF();
    }

    private static string ReadFromConsoleUntilEOF()
    {
        Console.WriteLine("Enter SQL (press Ctrl+Z on Windows or Ctrl+D on Unix when done):");
        
        var input = new System.Text.StringBuilder();
        string? line;
        
        while ((line = Console.ReadLine()) != null)
        {
            input.AppendLine(line);
        }
        
        return input.ToString();
    }

    private static void ShowUsage()
    {
        Console.WriteLine("SQL Query Analyzer");
        Console.WriteLine("==================");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  SqlQueryAnalyzer -f <file>     # Read from file");
        Console.WriteLine("  SqlQueryAnalyzer --file <file> # Read from file");
        Console.WriteLine("  SqlQueryAnalyzer               # Read from stdin/paste mode");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  SqlQueryAnalyzer -f query.sql");
        Console.WriteLine("  echo \"SELECT * FROM users;\" | SqlQueryAnalyzer");
        Console.WriteLine("  type query.sql | SqlQueryAnalyzer");
    }
}