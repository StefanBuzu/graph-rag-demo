using Anthropic.SDK;
using Anthropic.SDK.Constants;
using Anthropic.SDK.Messaging;
using Neo4j.Driver;

var apiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY")
    ?? throw new Exception("ANTHROPIC_API_KEY environment variable not set.");

var neo4jDriver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "testpassword"));
var claudeClient = new AnthropicClient(apiKey);

Console.WriteLine("Graph RAG Demo");
Console.WriteLine("==============");
Console.WriteLine("Ask a question about the people in the database.");
Console.WriteLine("Type 'exit' to quit.\n");

while (true)
{
    Console.Write("Your question: ");
    var question = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(question) || question.ToLower() == "exit")
        break;

    // Step 1: Ask Claude to generate a Cypher query from the user's question
    Console.WriteLine("\n[1] Generating Cypher query...");
    var cypherQuery = await GenerateCypherQuery(claudeClient, question);
    Console.WriteLine($"    Query: {cypherQuery}");

    // Step 2: Run the Cypher query against Neo4j
    Console.WriteLine("[2] Querying Neo4j...");
    var graphResults = await QueryNeo4j(neo4jDriver, cypherQuery);
    Console.WriteLine($"    Results: {graphResults}");

    // Step 3: Ask Claude to answer the original question using the graph results
    Console.WriteLine("[3] Generating answer...\n");
    var answer = await GenerateAnswer(claudeClient, question, graphResults);
    Console.WriteLine($"Answer: {answer}\n");
    Console.WriteLine(new string('-', 50) + "\n");
}

await neo4jDriver.DisposeAsync();

static async Task<string> GenerateCypherQuery(AnthropicClient client, string question)
{
    var schema = """
        Nodes: (:Person {name: string, age: int})
        Relationships:
          (:Person)-[:FRIENDS_WITH]->(:Person)
          (:Person)-[:WORKS_WITH]->(:Person)

        Important: relationships are stored directionally, so always match them
        without an arrow direction (e.g. -[:FRIENDS_WITH]-) to find connections
        in both directions.
        """;

    var messages = new List<Message>
    {
        new Message(RoleType.User, $"""
            You are a Neo4j Cypher expert. Given this graph schema:
            {schema}

            Convert this question to a Cypher query. Return ONLY the Cypher query, nothing else.
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

static async Task<string> QueryNeo4j(IDriver driver, string cypherQuery)
{
    try
    {
        await using var session = driver.AsyncSession();
        var result = await session.RunAsync(cypherQuery);
        var records = await result.ToListAsync();

        if (records.Count == 0)
            return "No results found.";

        var lines = records.Select(r => string.Join(", ", r.Keys.Select(k => $"{k}: {r[k]}")));
        return string.Join("\n", lines);
    }
    catch (Exception ex)
    {
        return $"Query error: {ex.Message}";
    }
}

static async Task<string> GenerateAnswer(AnthropicClient client, string question, string graphData)
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
