# Graph RAG Demo

A GraphRAG demo that turns a plain text document into a queryable knowledge graph, using **Neo4j** as the graph database and **Claude** as the AI layer.

**Live demo:** https://graph-rag-demo-u4rr.onrender.com

---

## How it works

### 1. Ingestion
When code is pushed to `master`, a GitHub Actions workflow reads `documents/speedy.txt` and sends it to Claude. Claude extracts all entities (companies, systems, roles, infrastructure) and the relationships between them, returning structured JSON. That JSON is then written into Neo4j Aura as a knowledge graph.

### 2. Asking a question
When a user asks a question in the chat UI, the following happens:

```
User question
      │
      ▼
Claude generates a Cypher query
      │
      ▼
Cypher query runs against Neo4j
      │
      ▼
Graph data returned
      │
      ▼
Claude answers the question using that data
      │
      ▼
Answer displayed in the UI
```

### 3. Governance report
The user can also request a governance report. The app runs three targeted Cypher queries to collect systems, roles, and infrastructure data, then sends it all to Claude to generate a structured report.

---

## The Knowledge Graph

This is how the extracted data looks inside Neo4j after ingestion:

![Neo4j Knowledge Graph](docs/Screenshot%202026-06-08%20192649.png)

Each node is an entity extracted from the document (company, system, component, infrastructure). The edges are the relationships between them.

---

## Tech stack

- **C# / .NET 10** — backend API and ingestion logic
- **Neo4j Aura** — cloud graph database
- **Claude Sonnet 4.6** — entity extraction, Cypher generation, question answering
- **GitHub Actions** — automated ingestion and report generation on every push
- **Render** — hosts both the frontend (static site) and backend (Docker)
