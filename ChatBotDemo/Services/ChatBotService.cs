using Microsoft.EntityFrameworkCore;
using ChatBotDemo.Data;
using ChatBotDemo.Models;
using Pgvector;

namespace ChatBotDemo.Services;

public class ChatBotService : IChatBotService
{
    private readonly ChatBotDbContext _context;
    private readonly IEmbeddingService _embeddingService;
    private readonly ILogger<ChatBotService> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public ChatBotService(
        ChatBotDbContext context,
        IEmbeddingService embeddingService,
        ILogger<ChatBotService> logger,
        IConfiguration configuration,
        HttpClient httpClient)
    {
        _context = context;
        _embeddingService = embeddingService;
        _logger = logger;
        _configuration = configuration;
        _httpClient = httpClient;
    }

    public async Task<List<Context>> GetAllContextsAsync()
    {
        return await _context.Contexts.OrderByDescending(c => c.CreatedAt).ToListAsync();
    }

    public async Task<Context?> GetContextByIdAsync(int id)
    {
        return await _context.Contexts.FindAsync(id);
    }

    public async Task<Context> AddContextAsync(string title, string content)
    {
        // Generate embedding for the content
        var embeddingArray = await _embeddingService.GenerateEmbeddingAsync(content);

        var context = new Context
        {
            Title = title,
            Content = content,
            Embedding = new Vector(embeddingArray),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Contexts.Add(context);
        await _context.SaveChangesAsync();
        return context;
    }

    public async Task<string> GetAnswerAsync(string question)
    {
        if (string.IsNullOrWhiteSpace(question))
        {
            return "Please provide a valid question.";
        }

        // Get all contexts with embeddings
        var contexts = await _context.Contexts
            .Where(c => c.Embedding != null)
            .ToListAsync();

        if (!contexts.Any())
        {
            return "No contexts available. Please add some contexts first.";
        }

        // Generate embedding for the question
        var questionEmbedding = await _embeddingService.GenerateEmbeddingAsync(question);
        var questionVector = new Vector(questionEmbedding);

        // Find the most similar context using vector cosine similarity
        // Using parameterized query with pgvector distance operator
        var bestMatch = await _context.Contexts
            .FromSqlInterpolated($@"
                SELECT * FROM ""Contexts""
                WHERE ""Embedding"" IS NOT NULL
                ORDER BY ""Embedding"" <=> {questionVector}
                LIMIT 1")
            .FirstOrDefaultAsync();

        if (bestMatch == null)
        {
            return "I couldn't find relevant information in the available contexts. Please try rephrasing your question or add more contexts.";
        }

        // Generate an answer using Gemini based on the context
        var answer = await GenerateAnswerWithGeminiAsync(question, bestMatch.Content);

        // Save the chat message
        var chatMessage = new ChatMessage
        {
            Question = question,
            Answer = answer,
            ContextId = bestMatch.Id,
            CreatedAt = DateTime.UtcNow
        };

        _context.ChatMessages.Add(chatMessage);
        await _context.SaveChangesAsync();

        return chatMessage.Answer;
    }

    public async Task RegenerateEmbeddingsAsync()
    {
        var contexts = await _context.Contexts
            .Where(c => c.Embedding == null)
            .ToListAsync();

        foreach (var context in contexts)
        {
            try
            {
                var embeddingArray = await _embeddingService.GenerateEmbeddingAsync(context.Content);
                context.Embedding = new Vector(embeddingArray);
                context.UpdatedAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating embedding for context {ContextId}", context.Id);
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task<List<ChatMessage>> GetChatHistoryAsync()
    {
        return await _context.ChatMessages
            .Include(m => m.Context)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    private async Task<string> GenerateAnswerWithGeminiAsync(string question, string context)
    {
        try
        {
            var apiKey = _configuration["Gemini:ApiKey"]
                ?? throw new InvalidOperationException("Gemini:ApiKey not found in configuration");

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";

            var prompt = $@"Based on the following context, answer the question concisely and accurately.
If the answer is not in the context, say so.

Context:
{context}

Question: {question}

Answer:";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            var json = System.Text.Json.JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            var responseJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Gemini API error: {StatusCode} - {Response}", response.StatusCode, responseJson);
                return $"Error generating answer: {response.StatusCode}";
            }

            var result = System.Text.Json.JsonSerializer.Deserialize<GeminiChatResponse>(responseJson, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var generatedText = result?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

            return generatedText ?? "I couldn't generate an answer based on the context.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating answer with Gemini for question: {Question}", question);
            return "An error occurred while generating the answer.";
        }
    }

    private class GeminiChatResponse
    {
        public List<Candidate>? Candidates { get; set; }
    }

    private class Candidate
    {
        public ContentData? Content { get; set; }
    }

    private class ContentData
    {
        public List<Part>? Parts { get; set; }
    }

    private class Part
    {
        public string? Text { get; set; }
    }
}

