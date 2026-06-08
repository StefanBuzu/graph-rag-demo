# Graph RAG Demo

A GraphRAG implementation using Neo4j Aura, Claude API, and C# — deployed on Render.

## Architecture

```
Browser (Render Static Site)
        │
        │  POST /api/ask  /  GET /api/report
        ▼
C# Web API (Render Web Service)
        │                  │
   Neo4j Aura         Claude API
  (knowledge graph)   (Sonnet 4.6)
```

On every push to `master`, GitHub Actions ingests `documents/speedy.txt` into Neo4j Aura and generates a fresh governance report.

## Deployment

### 1. Neo4j Aura
Sign up at [console.neo4j.io](https://console.neo4j.io) and create a free instance. Note the connection URI, username, and password.

### 2. GitHub Secrets
In your repo → Settings → Secrets → Actions, add:

| Secret | Value |
|--------|-------|
| `ANTHROPIC_API_KEY` | Your Anthropic API key |
| `NEO4J_URI` | e.g. `neo4j+s://xxxxxxxx.databases.neo4j.io` |
| `NEO4J_USER` | `neo4j` |
| `NEO4J_PASSWORD` | Your Aura password |

### 3. Render — Static Site (frontend)
- Connect this repo, set **Publish Directory** to `docs`
- No build command needed

### 4. Render — Web Service (backend)
- Connect this repo, Render detects the `Dockerfile` automatically
- Add the same 4 env vars from step 2
- Once live, copy the service URL

### 5. Wire them up
Paste the Web Service URL into the **Backend URL** field on the static site.

## Running locally

```powershell
# Start Neo4j
docker run -d --name neo4j -p 7474:7474 -p 7687:7687 -e NEO4J_AUTH=neo4j/testpassword neo4j:5

$env:ANTHROPIC_API_KEY = "your-key"

# Ingest document into Neo4j
dotnet run -- --ingest

# Interactive console (RAG Q&A + report)
dotnet run

# Run as web API on localhost:8080
dotnet run -- --serve
```

## Sample questions

- What systems does Speedy use?
- What infrastructure does Speedy run on?
- Who manages the ERP modules?
- What is accessible via REST API?
