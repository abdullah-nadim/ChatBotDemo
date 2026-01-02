using System.Text;
using System.Text.Json;

namespace ChatBotDemo.Services;

public class GeminiEmbeddingService : IEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GeminiEmbeddingService> _logger;
    private readonly string _apiKey;
    private const string Model = "text-embedding-004"; // Gemini's latest embedding model

    public GeminiEmbeddingService(IConfiguration configuration, ILogger<GeminiEmbeddingService> logger, HttpClient httpClient)
    {
        _apiKey = configuration["Gemini:ApiKey"]
            ?? throw new InvalidOperationException("Gemini:ApiKey not found in configuration");
        _httpClient = httpClient;
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
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{Model}:embedContent?key={_apiKey}";

            var requestBody = new
            {
                content = new
                {
                    parts = new[] { new { text } }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            var responseJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Gemini API error: {StatusCode} - {Response}", response.StatusCode, responseJson);
                throw new InvalidOperationException($"Gemini API returned {response.StatusCode}: {responseJson}");
            }

            _logger.LogDebug("Gemini API response: {Response}", responseJson);

            var result = JsonSerializer.Deserialize<GeminiEmbeddingResponse>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result?.Embedding?.Values == null || result.Embedding.Values.Length == 0)
            {
                _logger.LogError("Failed to parse embedding from response: {Response}", responseJson);
                throw new InvalidOperationException("No embedding returned from Gemini API");
            }

            _logger.LogInformation("Generated Gemini embedding with {Dimensions} dimensions", result.Embedding.Values.Length);

            // Gemini embeddings are 768 dimensions, but our DB expects 1536
            // Pad with zeros to match the expected dimension
            var embedding = result.Embedding.Values;
            if (embedding.Length < 1536)
            {
                var paddedEmbedding = new float[1536];
                Array.Copy(embedding, paddedEmbedding, embedding.Length);
                return paddedEmbedding;
            }

            return embedding;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating Gemini embedding for text: {Text}", text.Substring(0, Math.Min(100, text.Length)));
            throw;
        }
    }

    public async Task<List<float[]>> GenerateEmbeddingsAsync(List<string> texts)
    {
        if (texts == null || texts.Count == 0)
        {
            return new List<float[]>();
        }

        var embeddings = new List<float[]>();
        foreach (var text in texts)
        {
            embeddings.Add(await GenerateEmbeddingAsync(text));
        }

        return embeddings;
    }

    private class GeminiEmbeddingResponse
    {
        public EmbeddingData? Embedding { get; set; }
    }

    private class EmbeddingData
    {
        public float[]? Values { get; set; }
    }
}
