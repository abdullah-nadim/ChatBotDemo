# ChatBot Demo - Context-Aware Chatbot with RAG Architecture

A production-ready ASP.NET Core chatbot application that answers questions based on contextual information using **Retrieval-Augmented Generation (RAG)** architecture. The bot uses vector embeddings to find relevant context and Google Gemini AI to generate accurate, context-aware responses.

## Features

- **Context-Based Question Answering**: Retrieves relevant context using semantic search before generating answers
- **Vector Similarity Search**: Uses PostgreSQL with pgvector extension for efficient similarity matching
- **AI-Powered Responses**: Leverages Google Gemini 2.5 Flash for natural language generation
- **Semantic Embeddings**: Generates text embeddings using Gemini's text-embedding-004 model
- **Chat History**: Tracks all conversations with references to matched contexts
- **Context Management**: Add, update, and manage knowledge base contexts via REST API
- **Auto-Seeding**: Pre-populated with sample contexts about South Asian countries
- **RESTful API**: Clean API endpoints for integration with any frontend

## Technology Stack

- **Framework**: ASP.NET Core MVC (.NET 9.0)
- **Database**: PostgreSQL with pgvector extension
- **ORM**: Entity Framework Core 9.0
- **AI/ML**:
  - Google Gemini 2.5 Flash (text generation)
  - Google text-embedding-004 (embeddings)
- **Vector Search**: Pgvector for PostgreSQL
- **Architecture**: RAG (Retrieval-Augmented Generation)

## Architecture Overview

### How It Works

```
┌─────────────┐
│   User      │
│  Question   │
└──────┬──────┘
       │
       ▼
┌──────────────────────────────────────┐
│  1. Generate Question Embedding      │
│     (Gemini text-embedding-004)      │
└──────┬───────────────────────────────┘
       │
       ▼
┌──────────────────────────────────────┐
│  2. Vector Similarity Search         │
│     (pgvector cosine distance)       │
│     Find most relevant context       │
└──────┬───────────────────────────────┘
       │
       ▼
┌──────────────────────────────────────┐
│  3. Generate Answer with Context     │
│     (Gemini 2.5 Flash)               │
│     Prompt: Question + Context       │
└──────┬───────────────────────────────┘
       │
       ▼
┌──────────────────────────────────────┐
│  4. Save to Chat History             │
│     Store Q&A with context reference │
└──────┬───────────────────────────────┘
       │
       ▼
┌─────────────┐
│   Answer    │
│  Returned   │
└─────────────┘
```

### RAG Pipeline

1. **Indexing Phase** (happens when contexts are added):
   - Context content is converted to vector embeddings
   - Embeddings stored in PostgreSQL with pgvector

2. **Query Phase** (happens when user asks a question):
   - Question converted to vector embedding
   - Cosine similarity search finds most relevant context
   - LLM generates answer using both question and matched context
   - Response saved to chat history

## Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [PostgreSQL 15+](https://www.postgresql.org/download/) with pgvector extension
- [Google Gemini API Key](https://ai.google.dev/)

## Installation

### 1. Clone the Repository

```bash
git clone <repository-url>
cd ChatBotDemo
```

### 2. Install PostgreSQL and pgvector

#### On Ubuntu/Debian:
```bash
sudo apt update
sudo apt install postgresql postgresql-contrib
sudo apt install postgresql-15-pgvector
```

#### On macOS (using Homebrew):
```bash
brew install postgresql@15
brew install pgvector
```

#### On Windows:
- Download and install PostgreSQL from [official website](https://www.postgresql.org/download/windows/)
- Follow [pgvector installation guide](https://github.com/pgvector/pgvector#installation-notes)

### 3. Setup PostgreSQL Database

```bash
# Connect to PostgreSQL
psql -U postgres

# Create database
CREATE DATABASE chatbotdemo;

# Connect to the database
\c chatbotdemo

# Enable pgvector extension
CREATE EXTENSION vector;

# Exit psql
\q
```

### 4. Configure Application Settings

Create `appsettings.json` in the project root:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=chatbotdemo;Username=postgres;Password=your_password"
  },
  "Gemini": {
    "ApiKey": "your_gemini_api_key_here"
  }
}
```

**Get your Gemini API Key:**
1. Visit [Google AI Studio](https://ai.google.dev/)
2. Sign in with your Google account
3. Click "Get API Key"
4. Create a new API key or use an existing one

### 5. Restore Dependencies

```bash
dotnet restore
```

### 6. Run Database Migrations

```bash
dotnet ef database update
```

This will:
- Create all required tables (Contexts, ChatMessages)
- Enable vector column support
- Auto-seed sample contexts about Bangladesh, India, and Pakistan

### 7. Run the Application

```bash
dotnet run
```

The application will start at:
- HTTPS: `https://localhost:5001`
- HTTP: `http://localhost:5000`

## Configuration

### Embedding Service Options

The application supports multiple embedding services. Choose one in [Program.cs](Program.cs):

#### Option 1: Gemini Embedding Service (Recommended)
```csharp
builder.Services.AddHttpClient<GeminiEmbeddingService>();
builder.Services.AddScoped<IEmbeddingService, GeminiEmbeddingService>();
```

#### Option 2: OpenAI Embedding Service
```csharp
builder.Services.AddScoped<IEmbeddingService, OpenAIEmbeddingService>();
```
Add to `appsettings.json`:
```json
"OpenAI": {
  "ApiKey": "your_openai_api_key"
}
```

#### Option 3: Mock Embedding Service (Testing Only)
```csharp
builder.Services.AddScoped<IEmbeddingService, MockEmbeddingService>();
```

### Vector Dimensions

- Gemini embeddings: 768 dimensions (padded to 1536)
- OpenAI embeddings: 1536 dimensions
- Database column size: 1536 dimensions

## API Documentation

### Base URL
```
http://localhost:5000/api/chat
```

### Endpoints

#### 1. Ask a Question
```http
POST /api/chat/ask
Content-Type: application/json

{
  "question": "What is the capital of Bangladesh?"
}
```

**Response:**
```json
{
  "answer": "Based on the context provided, the capital of Bangladesh is Dhaka. It is described as the nation's political, financial, and cultural centre."
}
```

#### 2. Get All Contexts
```http
GET /api/chat/contexts
```

**Response:**
```json
[
  {
    "id": 1,
    "title": "Bangladesh",
    "content": "Bangladesh, officially the People's Republic...",
    "embedding": [...],
    "createdAt": "2024-01-03T10:30:00Z",
    "updatedAt": "2024-01-03T10:30:00Z"
  }
]
```

#### 3. Add New Context
```http
POST /api/chat/contexts
Content-Type: application/json

{
  "title": "Python Programming",
  "content": "Python is a high-level, interpreted programming language known for its simplicity and readability..."
}
```

**Response:**
```json
{
  "id": 4,
  "title": "Python Programming",
  "content": "Python is a high-level...",
  "embedding": [...],
  "createdAt": "2024-01-03T11:00:00Z",
  "updatedAt": "2024-01-03T11:00:00Z"
}
```

#### 4. Get Chat History
```http
GET /api/chat/history
```

**Response:**
```json
[
  {
    "id": 1,
    "question": "What is the capital of Bangladesh?",
    "answer": "Based on the context provided, the capital of Bangladesh is Dhaka...",
    "contextId": 1,
    "context": {
      "id": 1,
      "title": "Bangladesh"
    },
    "createdAt": "2024-01-03T10:35:00Z"
  }
]
```

#### 5. Regenerate Embeddings
```http
POST /api/chat/regenerate-embeddings
```

**Response:**
```json
{
  "message": "Embeddings regenerated successfully"
}
```

Use this endpoint to generate embeddings for contexts that don't have them yet.

## Usage Examples

### Example Questions (Based on Pre-seeded Data)

```
Q: What is the capital of Bangladesh?
A: The capital of Bangladesh is Dhaka, which is the nation's political, financial, and cultural centre.

Q: When did India gain independence?
A: India gained independence in 1947, when the British Indian Empire was partitioned into two independent dominions.

Q: What is Pakistan's official name?
A: Pakistan's official name is the Islamic Republic of Pakistan.

Q: Which country has the largest population in South Asia?
A: Based on the context, India is the most populous country with over 1.4 billion people as of 2023.

Q: Tell me about Bangladesh's geography.
A: Bangladesh shares land borders with India to the north, west, and east, and Myanmar to the southeast. It has a coastline along the Bay of Bengal and covers an area of 148,460 square kilometres.
```

### Testing with cURL

```bash
# Ask a question
curl -X POST http://localhost:5000/api/chat/ask \
  -H "Content-Type: application/json" \
  -d '{"question": "What is the capital of India?"}'

# Add a new context
curl -X POST http://localhost:5000/api/chat/contexts \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Machine Learning",
    "content": "Machine learning is a subset of artificial intelligence..."
  }'

# Get all contexts
curl http://localhost:5000/api/chat/contexts

# Get chat history
curl http://localhost:5000/api/chat/history
```

## Project Structure

```
ChatBotDemo/
├── Controllers/
│   ├── Api/
│   │   └── ChatController.cs          # REST API endpoints
│   └── HomeController.cs              # MVC controller
├── Data/
│   ├── ChatBotDbContext.cs            # EF Core DbContext
│   └── DbInitializer.cs               # Database seeding
├── Migrations/                         # EF Core migrations
├── Models/
│   ├── ChatMessage.cs                 # Chat history model
│   ├── Context.cs                     # Knowledge base context model
│   └── ErrorViewModel.cs              # Error handling
├── Services/
│   ├── IChatBotService.cs             # Chat service interface
│   ├── ChatBotService.cs              # Main chat logic
│   ├── IEmbeddingService.cs           # Embedding interface
│   ├── GeminiEmbeddingService.cs      # Gemini embeddings
│   ├── OpenAIEmbeddingService.cs      # OpenAI embeddings
│   └── MockEmbeddingService.cs        # Mock for testing
├── Views/                              # MVC views
├── wwwroot/                            # Static files
├── Program.cs                          # Application entry point
├── appsettings.json                    # Configuration
└── ChatBotDemo.csproj                  # Project file
```

## Key Components

### Models

**Context ([Models/Context.cs](Models/Context.cs))**
- Stores knowledge base information
- Contains vector embeddings for semantic search
- Fields: Id, Title, Content, Embedding (Vector), CreatedAt, UpdatedAt

**ChatMessage ([Models/ChatMessage.cs](Models/ChatMessage.cs))**
- Stores conversation history
- Links to the context used for generating the answer
- Fields: Id, Question, Answer, ContextId, CreatedAt

### Services

**ChatBotService ([Services/ChatBotService.cs](Services/ChatBotService.cs))**
- Core business logic for question answering
- Handles vector similarity search
- Generates answers using Gemini AI
- Manages chat history

**GeminiEmbeddingService ([Services/GeminiEmbeddingService.cs](Services/GeminiEmbeddingService.cs))**
- Generates 768-dimensional embeddings using Gemini
- Pads embeddings to 1536 dimensions for database compatibility
- Handles API communication with Google AI

## Troubleshooting

### Common Issues

#### 1. Gemini API 404 Error
```
Error: models/gemini-1.5-flash is not found
```
**Solution**: The code has been updated to use `gemini-2.5-flash`. Make sure you've rebuilt the application.

#### 2. Database Connection Error
```
Error: Password authentication failed for user "postgres"
```
**Solution**:
- Check your connection string in `appsettings.json`
- Ensure PostgreSQL is running: `sudo service postgresql status`
- Verify credentials: `psql -U postgres -h localhost`

#### 3. pgvector Extension Not Found
```
Error: type "vector" does not exist
```
**Solution**:
```sql
-- Connect to database
psql -U postgres -d chatbotdemo

-- Enable extension
CREATE EXTENSION vector;
```

#### 4. Gemini API Rate Limit
```
Error: Resource has been exhausted (e.g., check quota)
```
**Solution**:
- Check your [API quota](https://console.cloud.google.com/apis/api/generativelanguage.googleapis.com/quotas)
- Consider implementing rate limiting or caching
- Use the free tier limits responsibly

#### 5. No Contexts Available
```
Answer: No contexts available. Please add some contexts first.
```
**Solution**:
- Ensure database seeding ran successfully
- Check logs for seeding errors
- Manually add contexts via API: `POST /api/chat/contexts`

#### 6. Empty Embeddings
```
Error: Embedding is null
```
**Solution**:
- Run: `POST /api/chat/regenerate-embeddings`
- Check Gemini API key is valid
- Verify internet connectivity

## Performance Optimization

### Indexing for Vector Search

For better performance with large datasets, add indexes:

```sql
-- Create index on embedding column for faster similarity search
CREATE INDEX ON "Contexts" USING ivfflat (embedding vector_cosine_ops)
  WITH (lists = 100);

-- For better accuracy but slower build time
CREATE INDEX ON "Contexts" USING hnsw (embedding vector_cosine_ops);
```

### Caching Strategies

Consider implementing caching for:
- Frequently asked questions (Redis cache)
- Context embeddings (in-memory cache)
- Gemini API responses (with TTL)

## Future Enhancements

- [ ] Frontend UI with real-time chat interface
- [ ] Multi-context retrieval (top-k instead of top-1)
- [ ] Conversation memory across multiple questions
- [ ] File upload for bulk context addition (PDF, DOCX, TXT)
- [ ] Admin panel for context management
- [ ] User authentication and multi-tenancy
- [ ] Advanced filtering and search in contexts
- [ ] Export chat history as PDF/CSV
- [ ] Streaming responses for better UX
- [ ] Support for multiple languages
- [ ] Context versioning and rollback
- [ ] Analytics dashboard for usage metrics

## Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/your-feature`
3. Commit your changes: `git commit -m 'Add some feature'`
4. Push to the branch: `git push origin feature/your-feature`
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- [Google Gemini](https://ai.google.dev/) for AI capabilities
- [pgvector](https://github.com/pgvector/pgvector) for vector similarity search
- [Entity Framework Core](https://docs.microsoft.com/ef/core/) for ORM
- [ASP.NET Core](https://docs.microsoft.com/aspnet/core/) for the web framework

## Support

For issues, questions, or suggestions:
- Open an issue on GitHub
- Contact: [your-email@example.com]

## Version History

- **v1.0.0** (2026-01-03)
  - Initial release
  - RAG architecture with Gemini AI
  - PostgreSQL with pgvector
  - REST API endpoints
  - Auto-seeding with sample data
  - Gemini 2.5 Flash integration

---

Made with ❤️ using ASP.NET Core and Google Gemini AI
