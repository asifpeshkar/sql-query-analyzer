using SqlQueryAnalyzer.Services;
using SqlQueryAnalyzer.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddScoped<SqlAnalyzerService>();
builder.Services.AddScoped<SqlFormatter>();

// Add CORS for development
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseCors();

// POST /analyze endpoint
app.MapPost("/analyze", (AnalyzeRequest request, SqlAnalyzerService analyzer, SqlFormatter formatter) =>
{
    if (string.IsNullOrWhiteSpace(request.Sql))
    {
        return Results.BadRequest(new { error = "SQL content is required" });
    }

    try
    {
        var analysisResult = analyzer.Analyze(request.Sql);
        var formattedSql = formatter.Format(request.Sql);

        var response = new AnalyzeResponse(
            Issues: analysisResult.Issues.ToArray(),
            Formatted: formattedSql
        );

        return Results.Ok(response);
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "Analysis Error",
            detail: ex.Message,
            statusCode: 500
        );
    }
});

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

// Root endpoint with API info
app.MapGet("/", () => Results.Ok(new 
{ 
    name = "SQL Query Analyzer API",
    version = "1.0.0",
    endpoints = new[]
    {
        "GET /health - Health check",
        "POST /analyze - Analyze SQL query"
    }
}));

app.Run();

// API Models
public record AnalyzeRequest(string Sql);
public record AnalyzeResponse(Issue[] Issues, string Formatted);