using Anthropic.SDK;
using Anthropic.SDK.Constants;
using Anthropic.SDK.Messaging;
using Neo4j.Driver;
using System.Text.Json;
using System.Text.RegularExpressions;

var neo4jUri      = Environment.GetEnvironmentVariable("NEO4J_URI")      ?? "bolt://localhost:7687";
var neo4jUser     = Environment.GetEnvironmentVariable("NEO4J_USER")     ?? "neo4j";
var neo4jPassword = Environment.GetEnvironmentVariable("NEO4J_PASSWORD") ?? "testpassword";
var apiKey        = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY")
    ?? throw new Exception("ANTHROPIC_API_KEY environment variable not set.");

if (args.Contains("--serve"))
{
    var builder = WebApplication.CreateBuilder(
        args.Where(a => a != "--serve").ToArray());

    builder.Services.AddCors(options =>
        options.AddDefaultPolicy(policy =>
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

    var app = builder.Build();
    app.UseCors();

    var serveDriver = GraphDatabase.Driver(neo4jUri, AuthTokens.Basic(neo4jUser, neo4jPassword));
    var serveClient = new AnthropicClient(apiKey);

    app.MapGet("/health", () => Results.Ok("ok"));

    app.MapPost("/api/ask", async (QuestionRequest req) =>
    {
        var cypher  = await GenerateCypher(serveClient, req.Question);
        var results = await RunCypher(serveDriver, cypher);
        var answer  = await AnswerQuestion(serveClient, req.Question, results);
        return Results.Ok(new { answer });
    });

    app.MapGet("/api/report", async () =>
    {
        var report = await GenerateGovernanceReport(serveClient, serveDriver);
        return Results.Ok(new { content = report });
    });

    await app.RunAsync();
    await serveDriver.DisposeAsync();
    return;
}

var neo4jDriver = GraphDatabase.Driver(neo4jUri, AuthTokens.Basic(neo4jUser, neo4jPassword));
var claudeClient = new AnthropicClient(apiKey);

if (args.Contains("--ingest"))
{
    await IngestDocument(claudeClient, neo4jDriver);
    await neo4jDriver.DisposeAsync();
    return;
}

if (args.Contains("--report"))
{
    await GovernanceReport(claudeClient, neo4jDriver);
    await neo4jDriver.DisposeAsync();
    return;
}

Console.WriteLine("Speedy Knowledge Assistant");
Console.WriteLine("==========================");
Console.WriteLine("Type your question, 'report' for governance report, or 'exit' to quit.\n");

while (true)
{
    Console.Write("You: ");
    var input = Console.ReadLine()?.Trim();

    if (string.IsNullOrWhiteSpace(input) || input.ToLower() == "exit")
        break;

    if (input.ToLower() == "report")
    {
        await GovernanceReport(claudeClient, neo4jDriver);
        continue;
    }

    Console.WriteLine("\n[1] Generating Cypher query...");
    var cypher = await GenerateCypher(claudeClient, input);
    Console.WriteLine($"    {cypher}");

    Console.WriteLine("[2] Querying Neo4j...");
    var results = await RunCypher(neo4jDriver, cypher);
    Console.WriteLine($"    {results}");

    Console.WriteLine("[3] Generating answer...\n");
    var answer = await AnswerQuestion(claudeClient, input, results);
    Console.WriteLine($"Assistant: {answer}\n");
    Console.WriteLine(new string('-', 50) + "\n");
}

await neo4jDriver.DisposeAsync();

// ── Ingestion ────────────────────────────────────────────────────────────────

static async Task IngestDocument(AnthropicClient client, IDriver driver)
{
    var documentPath = Path.Combine(AppContext.BaseDirectory, "documents", "speedy.txt");

    if (!File.Exists(documentPath))
    {
        Console.WriteLine($"Document not found: {documentPath}");
        return;
    }

    var text = await File.ReadAllTextAsync(documentPath);
    Console.WriteLine("Document loaded. Extracting entities and relationships...");

    var messages = new List<Message>
    {
        new Message(RoleType.User, $$"""
            Extract all entities and relationships from this text and return them as JSON.

            Use this exact format:
            {
              "nodes": [
                {"id": "unique_snake_case_id", "label": "Category", "name": "Display Name"}
              ],
              "relationships": [
                {"from_id": "id1", "to_id": "id2", "type": "RELATIONSHIP_TYPE"}
              ]
            }

            Labels to use: Company, Role, System, Component, Technology, Infrastructure, Platform
            Relationship types: MANAGES, USES, RUNS_ON, CONTAINS, EXTRACTS_FROM, SEPARATES, ACCESSIBLE_VIA

            Return ONLY the JSON, no explanation, no markdown.

            Text:
            {{text}}
            """)
    };

    var response = await client.Messages.GetClaudeMessageAsync(new MessageParameters
    {
        Model = AnthropicModels.Claude46Sonnet,
        MaxTokens = 2000,
        Messages = messages
    });

    var json = response.Content.OfType<TextContent>().First().Text.Trim();
    json = Regex.Replace(json, @"```json\s*|\s*```", "").Trim();

    Console.WriteLine("Entities extracted. Storing in Neo4j...");

    using var doc = JsonDocument.Parse(json);
    var root = doc.RootElement;

    await using var session = driver.AsyncSession();
    await session.RunAsync("MATCH (n) DETACH DELETE n");

    foreach (var node in root.GetProperty("nodes").EnumerateArray())
    {
        var id    = node.GetProperty("id").GetString();
        var label = node.GetProperty("label").GetString();
        var name  = node.GetProperty("name").GetString();

        await session.RunAsync(
            $"MERGE (n:{label} {{id: $id}}) SET n.name = $name",
            new { id, name });
    }

    foreach (var rel in root.GetProperty("relationships").EnumerateArray())
    {
        var fromId = rel.GetProperty("from_id").GetString();
        var toId   = rel.GetProperty("to_id").GetString();
        var type   = rel.GetProperty("type").GetString();

        await session.RunAsync(
            $"MATCH (a {{id: $fromId}}), (b {{id: $toId}}) MERGE (a)-[:{type}]->(b)",
            new { fromId, toId });
    }

    Console.WriteLine("Knowledge graph ready.");
}

// ── Governance report ────────────────────────────────────────────────────────

static async Task GovernanceReport(AnthropicClient client, IDriver driver)
{
    Console.WriteLine("\nRunning governance report agent...");
    Console.WriteLine("[Agent - Step 1] Collecting systems and dependencies...");
    Console.WriteLine("[Agent - Step 2] Collecting roles and responsibilities...");
    Console.WriteLine("[Agent - Step 3] Collecting infrastructure...");
    Console.WriteLine("[Agent - Step 4] Generating report...\n");
    Console.WriteLine(new string('=', 50));

    var report = await GenerateGovernanceReport(client, driver);

    Console.WriteLine(report);
    Console.WriteLine(new string('=', 50) + "\n");

    var outputPath = Path.Combine(Directory.GetCurrentDirectory(), "governance-report.md");
    await File.WriteAllTextAsync(outputPath, report);
    Console.WriteLine($"Report saved to: {outputPath}");
}

static async Task<string> GenerateGovernanceReport(AnthropicClient client, IDriver driver)
{
    var systems = await RunCypher(driver,
        "MATCH (s)-[r]->(t) RETURN s.name AS source, type(r) AS relationship, t.name AS target");
    var roles = await RunCypher(driver,
        "MATCH (r:Role)-[rel]->(s) RETURN r.name AS role, type(rel) AS action, s.name AS target");
    var infra = await RunCypher(driver,
        "MATCH (n:Infrastructure) RETURN n.name AS name");

    var messages = new List<Message>
    {
        new Message(RoleType.User, $"""
            You are a governance consultant. Based on the data below from Speedy's knowledge graph,
            write a concise governance report for top management.

            Structure the report as:
            1. Executive Summary
            2. Key Systems Overview
            3. Dependencies & Risks
            4. Recommendations

            Systems and relationships:
            {systems}

            Roles and responsibilities:
            {roles}

            Infrastructure:
            {infra}
            """)
    };

    var response = await client.Messages.GetClaudeMessageAsync(new MessageParameters
    {
        Model = AnthropicModels.Claude46Sonnet,
        MaxTokens = 1500,
        Messages = messages
    });

    return response.Content.OfType<TextContent>().First().Text.Trim();
}

// ── Helpers ──────────────────────────────────────────────────────────────────

static async Task<string> GenerateCypher(AnthropicClient client, string question)
{
    var messages = new List<Message>
    {
        new Message(RoleType.User, $"""
            You are a Neo4j Cypher expert. Convert this question to a Cypher query.

            Node labels: Company, Role, System, Component, Technology, Infrastructure, Platform
            Node properties: id, name
            Relationships: MANAGES, USES, RUNS_ON, CONTAINS, EXTRACTS_FROM, SEPARATES, ACCESSIBLE_VIA

            Always match relationships without direction (e.g. -[:USES]-) to find connections in both directions.
            Return ONLY the Cypher query, nothing else.

            Question: {question}
            """)
    };

    var response = await client.Messages.GetClaudeMessageAsync(new MessageParameters
    {
        Model = AnthropicModels.Claude46Sonnet,
        MaxTokens = 300,
        Messages = messages
    });

    return response.Content.OfType<TextContent>().First().Text.Trim();
}

static async Task<string> RunCypher(IDriver driver, string cypher)
{
    try
    {
        await using var session = driver.AsyncSession();
        var result  = await session.RunAsync(cypher);
        var records = await result.ToListAsync();

        if (records.Count == 0) return "No results found.";

        var lines = records.Select(r => string.Join(", ", r.Keys.Select(k => $"{k}: {r[k]}")));
        return string.Join("\n", lines);
    }
    catch (Exception ex)
    {
        return $"Query error: {ex.Message}";
    }
}

static async Task<string> AnswerQuestion(AnthropicClient client, string question, string graphData)
{
    var messages = new List<Message>
    {
        new Message(RoleType.User, $"""
            Answer the following question using only the data provided. Be concise.

            Question: {question}

            Data from the knowledge graph:
            {graphData}
            """)
    };

    var response = await client.Messages.GetClaudeMessageAsync(new MessageParameters
    {
        Model = AnthropicModels.Claude46Sonnet,
        MaxTokens = 500,
        Messages = messages
    });

    return response.Content.OfType<TextContent>().First().Text.Trim();
}

record QuestionRequest(string Question);
