# SQL Query Analyzer - Web API

This project includes both a CLI tool and a minimal Web API for analyzing SQL queries.

## Projects Structure

```
SqlQueryAnalyzer.sln
├── src/SqlQueryAnalyzer/          # Console CLI Application
│   ├── Program.cs                 # CLI entry point
│   ├── Models/                    # Shared models
│   ├── Services/                  # Shared services
│   └── Utilities/                 # Shared utilities
└── src/SqlQueryAnalyzer.Api/      # Web API
    ├── Program.cs                 # API entry point
    ├── Properties/launchSettings.json
    └── SqlQueryAnalyzer.Api.csproj
```

## Running the Applications

### CLI Tool
```bash
# Build and run CLI
dotnet build
dotnet run --project src/SqlQueryAnalyzer -- -f query.sql
echo "SELECT * FROM users;" | dotnet run --project src/SqlQueryAnalyzer
```

### Web API
```bash
# Run the API
dotnet run --project src/SqlQueryAnalyzer.Api

# API will be available at:
# - HTTP: http://localhost:5000
# - HTTPS: https://localhost:5001
```

## API Endpoints

### GET /
Returns API information and available endpoints.

**Response:**
```json
{
  "name": "SQL Query Analyzer API",
  "version": "1.0.0",
  "endpoints": [
    "GET /health - Health check",
    "POST /analyze - Analyze SQL query"
  ]
}
```

### GET /health
Health check endpoint.

**Response:**
```json
{
  "status": "healthy",
  "timestamp": "2025-10-07T10:48:45.2769688Z"
}
```

### POST /analyze
Analyzes SQL query and returns issues and formatted SQL.

**Request:**
```json
{
  "sql": "SELECT * FROM users WHERE active = 1; UPDATE users SET status = 'active'"
}
```

**Response:**
```json
{
  "issues": [
    {
      "ruleId": "SQL001",
      "severity": 1,
      "message": "SELECT * should be avoided",
      "suggestion": "Specify explicit column names",
      "statementIndex": 0,
      "span": null
    },
    {
      "ruleId": "SQL021",
      "severity": 2,
      "message": "UPDATE without WHERE clause",
      "suggestion": "Add WHERE clause to limit scope",
      "statementIndex": 1,
      "span": null
    }
  ],
  "formatted": "SELECT\n  *\n    FROM users\n    WHERE active = 1;\n\nUPDATE users\nSET status = 'active';"
}
```

## Issue Severity Levels
- `0` = Info
- `1` = Warning  
- `2` = Error

## Example Usage

### PowerShell
```powershell
# Test the API
$body = @{ sql = "SELECT * FROM users; DROP TABLE temp" } | ConvertTo-Json
$response = Invoke-RestMethod -Uri "http://localhost:5000/analyze" -Method POST -Body $body -ContentType "application/json"

# Display results
Write-Host "Issues:"
$response.issues | ForEach-Object { 
    Write-Host "- [$($_.severity)] $($_.ruleId): $($_.message)" 
}
Write-Host "`nFormatted SQL:"
Write-Host $response.formatted
```

### curl
```bash
# Test the analyze endpoint
curl -X POST http://localhost:5000/analyze \
  -H "Content-Type: application/json" \
  -d '{"sql": "SELECT * FROM users; DROP TABLE temp"}'

# Test the health endpoint
curl http://localhost:5000/health
```

### JavaScript/Fetch
```javascript
const response = await fetch('http://localhost:5000/analyze', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    sql: 'SELECT * FROM users WHERE id = 1'
  })
});

const result = await response.json();
console.log('Issues:', result.issues);
console.log('Formatted SQL:', result.formatted);
```

## Development Configuration

The API includes development-friendly features:
- **CORS enabled** for all origins in development
- **Developer exception page** for detailed error information
- **Hot reload** support with `dotnet watch`

### Launch Profiles

Two launch profiles are configured in `launchSettings.json`:

1. **Development** (default):
   - HTTP: http://localhost:5000
   - HTTPS: https://localhost:5001
   - Opens browser automatically
   - Development environment

2. **Production**:
   - HTTP: http://localhost:8080
   - No browser launch
   - Production environment

## Shared Services

Both CLI and API use the same core services:
- `SqlAnalyzerService` - Performs SQL analysis with all rule checks
- `SqlFormatter` - Formats SQL with proper indentation and structure
- `SqlUtils` - Utility functions for SQL parsing

This ensures consistency between CLI and API results.

## Building and Deployment

```bash
# Build entire solution
dotnet build

# Build specific projects
dotnet build src/SqlQueryAnalyzer
dotnet build src/SqlQueryAnalyzer.Api

# Publish for deployment
dotnet publish src/SqlQueryAnalyzer.Api -c Release -o ./publish/api
dotnet publish src/SqlQueryAnalyzer -c Release -o ./publish/cli
```