# Graph RAG Demo

A simple GraphRAG implementation using Neo4j and the Claude API, built in C#.

## What it does

Demonstrates the GraphRAG pattern — using a knowledge graph as the retrieval source for an LLM:

1. You ask a question in natural language
2. Claude converts it to a Cypher query
3. The query runs against a Neo4j graph database
4. Claude answers your question using the retrieved graph data

## Prerequisites

- [.NET 10](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- An [Anthropic API key](https://console.anthropic.com)

## Setup

**1. Start Neo4j in Docker:**
```bash
docker run -d --name neo4j-test -p 7474:7474 -p 7687:7687 -e NEO4J_AUTH=neo4j/testpassword neo4j:5
```

**2. Set your API key:**
```powershell
$env:ANTHROPIC_API_KEY = "your-key-here"
```

**3. Run the app:**
```powershell
dotnet run
```

## Sample questions to try

- Who are Alice's friends?
- Who does Alice work with?
- How is Bob connected to Dave?
