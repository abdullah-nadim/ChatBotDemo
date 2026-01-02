# ChatBot Demo - Setup Guide

## Overview
This is a minimal chatbot demo that uses PostgreSQL to store contexts and answer questions based on those contexts.

## Approach for Minimal Demo

Since you're building a minimal demo with 2-3 contexts, I've implemented a **simplified approach**:

### Current Implementation (Simple Text Search)
- ✅ PostgreSQL for data storage
- ✅ Simple keyword-based matching (no vector embeddings needed)
- ✅ RESTful API endpoints
- ✅ Web UI for chat interaction
- ✅ Chat history tracking

### Why This Approach?
For a minimal demo with only 2-3 contexts:
- **No vector DB needed** - Simple keyword matching works well
- **No embeddings required** - Saves API costs and complexity
- **PostgreSQL full-text search** - Can be enhanced later if needed
- **Fast to implement** - Get it working quickly

### Future Enhancements (Optional)
If you want to scale later, you can:
1. Add **pgvector extension** to PostgreSQL for vector similarity search
2. Integrate **OpenAI embeddings API** for better semantic matching
3. Add **chunking logic** for larger contexts
4. Implement **hybrid search** (vector + keyword)

## Setup Instructions

### 1. Install PostgreSQL
Make sure PostgreSQL is installed and running on your machine.

### 2. Create Database
```sql
CREATE DATABASE ChatBotDemo;
```

### 3. Update Connection String
Edit `appsettings.json` and update the connection string:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=ChatBotDemo;Username=postgres;Password=YOUR_PASSWORD"
}
```

### 4. Install NuGet Packages
The following packages are already added to `.csproj`:
- `Microsoft.EntityFrameworkCore` (9.0.0)
- `Microsoft.EntityFrameworkCore.Design` (9.0.0)
- `Npgsql.EntityFrameworkCore.PostgreSQL` (9.0.2)

Run:
```bash
dotnet restore
```

### 5. Create Database Migration
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 6. Add Sample Contexts
You can add contexts via the API:
```bash
POST /api/Chat/contexts
Content-Type: application/json

{
  "title": "About Our Company",
  "content": "Our company specializes in software development and provides innovative solutions for businesses."
}
```

Or you can seed the database programmatically (see next section).

## API Endpoints

### POST `/api/Chat/ask`
Ask a question to the chatbot.
```json
{
  "question": "What does your company do?"
}
```

### GET `/api/Chat/contexts`
Get all available contexts.

### POST `/api/Chat/contexts`
Add a new context.
```json
{
  "title": "Context Title",
  "content": "Context content here..."
}
```

### GET `/api/Chat/history`
Get chat history.

## Project Structure

```
ChatBotDemo/
├── Controllers/
│   ├── Api/
│   │   └── ChatController.cs      # API endpoints
│   └── HomeController.cs
├── Data/
│   └── ChatBotDbContext.cs        # EF Core DbContext
├── Models/
│   ├── Context.cs                  # Context entity
│   └── ChatMessage.cs              # Chat message entity
├── Services/
│   ├── IChatBotService.cs          # Service interface
│   └── ChatBotService.cs           # Business logic
└── Views/
    └── Home/
        └── Index.cshtml            # Chat UI
```

## How It Works

1. **Context Storage**: Contexts are stored in PostgreSQL `Contexts` table
2. **Question Matching**: When a question is asked, the service:
   - Splits the question into keywords
   - Searches all contexts for matching keywords
   - Returns the context with the highest relevance score
3. **Answer Generation**: Returns the matching context content as the answer
4. **History**: All Q&A pairs are saved in `ChatMessages` table

## Testing

1. Start the application: `dotnet run`
2. Navigate to `https://localhost:5001` (or your configured port)
3. Add contexts via API or seed data
4. Ask questions in the chat interface

## Next Steps (Optional Enhancements)

1. **Add Seed Data**: Create a migration or initializer to add sample contexts
2. **Improve Matching**: Implement PostgreSQL full-text search (tsvector/tsquery)
3. **Add Vector Search**: Install pgvector and add embedding generation
4. **Better UI**: Enhance the chat interface with better styling
5. **Context Management**: Add UI for managing contexts

