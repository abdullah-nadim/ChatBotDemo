using OpenAI;
using OpenAI.Embeddings;

namespace ChatBotDemo.Services;

public class OpenAIEmbeddingService : IEmbeddingService
{
    private readonly EmbeddingClient _client;
    private readonly ILogger<OpenAIEmbeddingService> _logger;
    private const string Model = "text-embedding-3-small"; // or "text-embedding-ada-002"

    public OpenAIEmbeddingService(IConfiguration configuration, ILogger<OpenAIEmbeddingService> logger)
    {
        var apiKey = configuration["OpenAI:ApiKey"]
            ?? throw new InvalidOperationException("OpenAI:ApiKey not found in configuration");

        _client = new EmbeddingClient(Model, apiKey);
        _logger = logger;
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Text cannot be null or empty", nameof(text));
        }

        try
        {
            var embedding = await _client.GenerateEmbeddingAsync(text);
            return embedding.Value.ToFloats().ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating embedding for text: {Text}", text);
            throw;
        }
    }

    public async Task<List<float[]>> GenerateEmbeddingsAsync(List<string> texts)
    {
        if (texts == null || texts.Count == 0)
        {
            return new List<float[]>();
        }

        try
        {
            var embeddings = new List<float[]>();

            foreach (var text in texts)
            {
                var embedding = await _client.GenerateEmbeddingAsync(text);
                embeddings.Add(embedding.Value.ToFloats().ToArray());
            }

            return embeddings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating embeddings for {Count} texts", texts.Count);
            throw;
        }
    }
}



