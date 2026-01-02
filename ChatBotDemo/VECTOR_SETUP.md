# Vector Database Setup Guide

## Overview
The chatbot now uses **vector embeddings** with PostgreSQL's **pgvector** extension for semantic similarity search. This provides much better context matching than simple keyword search.

## Prerequisites

1. **PostgreSQL** (version 11 or higher)
2. **pgvector extension** installed in PostgreSQL
3. **OpenAI API Key** for generating embeddings

## Step 1: Install pgvector Extension

### On Windows (using pgAdmin or psql):

```sql
-- Connect to your PostgreSQL database
CREATE EXTENSION IF NOT EXISTS vector;
```

### On Linux/Mac:

```bash
# Install pgvector (method depends on your PostgreSQL installation)
# For example, with apt:
sudo apt-get install postgresql-15-pgvector

# Then enable it in your database:
psql -d ChatBotDemo -c "CREATE EXTENSION IF NOT EXISTS vector;"
```

### Verify Installation:

```sql
SELECT * FROM pg_extension WHERE extname = 'vector';
```

## Step 2: Configure OpenAI API Key

1. Get your OpenAI API key from: https://platform.openai.com/api-keys
2. Update `appsettings.json`:

```json
{
  "OpenAI": {
    "ApiKey": "sk-your-actual-api-key-here"
  }
}
```

**Important:** For production, use User Secrets or Environment Variables instead of storing the key in `appsettings.json`.

### Using Environment Variables (Recommended):

```bash
# Windows PowerShell
$env:OpenAI__ApiKey = "sk-your-api-key"

# Linux/Mac
export OpenAI__ApiKey="sk-your-api-key"
```

Or add to `appsettings.Development.json` (which should be in `.gitignore`):

```json
{
  "OpenAI": {
    "ApiKey": "sk-your-api-key"
  }
}
```

## Step 3: Create Database Migration

Run the following commands to create and apply the migration:

```bash
# Create migration
dotnet ef migrations add AddVectorEmbeddings

# Apply migration
dotnet ef database update
```

This will:
- Add the `Embedding` column to the `Contexts` table
- Create an HNSW index for fast vector similarity search

## Step 4: Generate Embeddings for Existing Contexts

When you run the application, it will automatically:
1. Seed the database with contexts (if they don't exist)
2. Generate embeddings for all contexts that don't have them

You can also manually trigger embedding generation via API:

```bash
POST /api/Chat/regenerate-embeddings
```

## How It Works

1. **Context Ingestion:**
   - When a context is added, its content is sent to OpenAI's embedding API
   - The embedding (1536-dimensional vector) is stored in PostgreSQL

2. **Question Processing:**
   - When a question is asked, it's converted to an embedding
   - PostgreSQL uses cosine similarity (`<=>` operator) to find the most similar context
   - The context with the highest similarity score is returned

3. **Vector Search:**
   - Uses HNSW (Hierarchical Navigable Small World) index for fast approximate nearest neighbor search
   - Much faster than linear search, especially with many contexts

## Testing

1. Start the application: `dotnet run`
2. The app will automatically generate embeddings for seed contexts
3. Ask questions in the chat interface - it should now find semantically similar contexts, not just keyword matches

## Troubleshooting

### Error: "extension vector does not exist"
- Make sure pgvector is installed and enabled in your database
- Run: `CREATE EXTENSION IF NOT EXISTS vector;`

### Error: "OpenAI:ApiKey not found"
- Add your OpenAI API key to `appsettings.json` or environment variables
- Make sure the key starts with `sk-`

### Embeddings not generating
- Check your OpenAI API key is valid
- Check your OpenAI account has credits/quota
- Check application logs for detailed error messages

### Slow queries
- Make sure the HNSW index was created: `\d+ "Contexts"` in psql
- The index should appear on the `Embedding` column

## Cost Considerations

- OpenAI embeddings API is very affordable:
  - `text-embedding-3-small`: $0.02 per 1M tokens
  - Each context embedding: ~$0.00001-0.0001 (very cheap)
  - Question embedding: same cost per question

For 3 contexts with ~1000 words each, you're looking at pennies per month.

## Alternative Embedding Models

If you want to use a different model, edit `Services/OpenAIEmbeddingService.cs`:

```csharp
private const string Model = "text-embedding-ada-002"; // Alternative model
```

Available models:
- `text-embedding-3-small` (1536 dimensions) - Recommended, cheapest
- `text-embedding-3-large` (3072 dimensions) - More accurate, more expensive
- `text-embedding-ada-002` (1536 dimensions) - Older model

Note: If you change the model dimensions, update the vector column size in `ChatBotDbContext.cs`:
```csharp
entity.Property(e => e.Embedding)
      .HasColumnType("vector(3072)") // For large model
```



