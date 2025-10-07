# 🔍 SQL Query Analyzer

A powerful .NET 8 application that analyzes SQL queries for potential issues and provides intelligent formatting suggestions. Features both a command-line interface and a modern web UI.

**Created by Asif Peshkar @ NexumSoftware**

## ✨ Features

- **🎯 Comprehensive SQL Analysis**: 13+ built-in rules to detect common SQL issues
- **🎨 Modern Web UI**: Beautiful two-pane interface for easy SQL analysis
- **⚡ CLI Tool**: Command-line interface for integration into workflows
- **🔧 REST API**: HTTP API for programmatic access (local development)
- **📱 Responsive Design**: Works on desktop, tablet, and mobile devices
- **🚀 Static Deployment**: Fully client-side, perfect for Vercel/Netlify
- **⚡ Zero Dependencies**: No server required for web UI

## 🛠️ Technology Stack

- **.NET 8.0**: Modern C# framework (CLI/API)
- **Pure JavaScript**: Client-side SQL analysis engine
- **HTML5/CSS3**: Modern web technologies
- **Regex-based Analysis**: No external dependencies

## 📋 SQL Rules Analyzed

| Rule ID | Description | Severity |
|---------|-------------|----------|
| SQL001 | SELECT * usage | Warning |
| SQL002 | Missing WHERE clause in UPDATE | Error |
| SQL003 | Missing WHERE clause in DELETE | Error |
| SQL004 | Hardcoded values in queries | Warning |
| SQL005 | Missing table aliases | Info |
| SQL006 | Inconsistent naming conventions | Warning |
| SQL007 | Missing indexes hints | Info |
| SQL008 | Subquery performance issues | Warning |
| SQL009 | UNION vs UNION ALL | Warning |
| SQL010 | ORDER BY with LIMIT/TOP | Info |
| ... | *and 34 more rules* | ... |

## 🚀 Quick Start

### Prerequisites
- .NET 8.0 SDK
- Python 3.x (for local web server)

### Running the Application

#### 1. Web UI (Recommended - Fully Static)
```bash
# Option A: Local development server
python -m http.server 8080
# Then open http://localhost:8080

# Option B: Deploy to Vercel
# Just push to GitHub and connect to Vercel - it's fully static!

# Option C: Open index.html directly in browser
# Double-click index.html or open file:// URL
```

#### 2. Command Line Interface
```bash
cd src/SqlQueryAnalyzer

# Analyze a SQL file
dotnet run -- -f path/to/your/query.sql

# Or pipe SQL directly
echo "SELECT * FROM users" | dotnet run
```

#### 3. REST API (Local Development)
```bash
# Start the API server (for advanced features)
cd src/SqlQueryAnalyzer.Api
dotnet run

# Test with curl
curl -X POST http://localhost:5000/analyze \
  -H "Content-Type: application/json" \
  -d '{"sql": "SELECT * FROM users"}'
```

## 📁 Project Structure

```
SqlAnalyzer/
├── src/
│   ├── SqlQueryAnalyzer/           # Console application
│   │   ├── Program.cs              # CLI entry point
│   │   ├── Models/                 # Data models
│   │   ├── Services/               # Core analysis services
│   │   └── Utilities/              # Helper utilities
│   └── SqlQueryAnalyzer.Api/       # Web API
│       ├── Program.cs              # API entry point
│       └── Properties/
├── index.html                      # Web UI
├── api-test.html                   # API testing page
├── API-USAGE.md                    # API documentation
├── test.sql                        # Sample SQL file
└── vercel.json                     # Vercel deployment config
```

## 🌐 Web UI Features

- **Two-Pane Layout**: SQL input on the left, analysis results on the right
- **Sample Queries**: Quick-start examples for testing
- **Real-time Stats**: Character and line count display
- **Issue Categorization**: Color-coded severity levels (Info, Warning, Error)
- **Formatted Output**: Beautifully formatted SQL with proper indentation
- **Keyboard Shortcuts**: Ctrl+Enter to analyze quickly
- **Mobile Responsive**: Works seamlessly on all devices

## 🔧 API Endpoints

### `POST /analyze`
Analyzes SQL query and returns issues with formatted SQL.

**Request:**
```json
{
  "sql": "SELECT * FROM users WHERE active = 1"
}
```

**Response:**
```json
{
  "issues": [
    {
      "ruleId": "SQL001",
      "severity": 1,
      "message": "Avoid using SELECT *",
      "suggestion": "Specify explicit column names",
      "statementIndex": 0
    }
  ],
  "formatted": "SELECT *\nFROM users\nWHERE active = 1"
}
```

### `GET /health`
Health check endpoint.

## 🚀 Deployment

### Static Hosting (Vercel/Netlify/GitHub Pages)
1. Push your code to GitHub
2. Connect to your hosting provider
3. Deploy - it's fully static with zero configuration needed!
4. **Live Demo**: Works instantly without any server setup

### Local Development
```bash
# Web UI only (static)
python -m http.server 8080

# Full development with API
cd src/SqlQueryAnalyzer.Api && dotnet run
```

## 🧪 Testing

### Sample SQL Queries
```sql
-- Query with issues
SELECT * FROM users;
UPDATE users SET status = 'inactive';
DELETE FROM logs;

-- Well-formatted query
SELECT 
    u.id,
    u.name,
    u.email
FROM users u
WHERE u.active = 1
ORDER BY u.name;
```

## 📖 Documentation

- [API Usage Guide](API-USAGE.md) - Comprehensive API documentation
- [Interactive API Test](api-test.html) - Test API endpoints directly

## 🤝 Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👨‍💻 Author

**Asif Peshkar**  
NexumSoftware  

## 🙏 Acknowledgments

- Built with .NET 8 and modern web technologies
- Inspired by SQL best practices and code analysis tools
- Designed for developers who care about SQL quality

---

⭐ **Star this repository if you find it helpful!** ⭐

# Run with file input
dotnet run -- -f test.sql

# Run with stdin
echo "SELECT * FROM table;" | dotnet run
```

## Example Output

```
Issues found:
RuleId: SQL001
Severity: Warning
Message: SELECT * should be avoided
Suggestion: Specify explicit column names instead of using SELECT *

RuleId: SQL002
Severity: Error
Message: UPDATE statement without WHERE clause
Suggestion: Add WHERE clause to limit affected rows

=== FORMATTED SQL ===
SELECT
    *
FROM users;

UPDATE users
SET status = 'active';
```

## Requirements

- .NET 8.0 or later
- No external dependencies