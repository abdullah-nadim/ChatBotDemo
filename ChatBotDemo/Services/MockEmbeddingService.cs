namespace ChatBotDemo.Services;

public class MockEmbeddingService : IEmbeddingService
{
    private readonly ILogger<MockEmbeddingService> _logger;
    private const int EmbeddingDimension = 1536; // Same as text-embedding-3-small

    public MockEmbeddingService(ILogger<MockEmbeddingService> logger)
    {
        _logger = logger;
        _logger.LogWarning("Using MockEmbeddingService - embeddings will be randomly generated for testing purposes only!");
    }

    public Task<float[]> GenerateEmbeddingAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Text cannot be null or empty", nameof(text));
        }

        _logger.LogInformation("Generating mock embedding for text of length {Length}", text.Length);

        // Generate deterministic "random" embedding based on text hash
        // This ensures same text always gets same embedding
        var hashCode = GetStableHashCode(text);
        var random = new Random(hashCode);

        var embedding = new float[EmbeddingDimension];
        for (int i = 0; i < EmbeddingDimension; i++)
        {
            // Generate values between -1 and 1 (typical range for normalized embeddings)
            embedding[i] = (float)(random.NextDouble() * 2 - 1);
        }

        // Normalize the vector (make it unit length)
        NormalizeVector(embedding);

        return Task.FromResult(embedding);
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

    private static int GetStableHashCode(string str)
    {
        unchecked
        {
            int hash1 = 5381;
            int hash2 = hash1;

            for (int i = 0; i < str.Length && str[i] != '\0'; i += 2)
            {
                hash1 = ((hash1 << 5) + hash1) ^ str[i];
                if (i == str.Length - 1 || str[i + 1] == '\0')
                    break;
                hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
            }

            return hash1 + (hash2 * 1566083941);
        }
    }

    private static void NormalizeVector(float[] vector)
    {
        double sumOfSquares = 0;
        for (int i = 0; i < vector.Length; i++)
        {
            sumOfSquares += vector[i] * vector[i];
        }

        double magnitude = Math.Sqrt(sumOfSquares);
        if (magnitude > 0)
        {
            for (int i = 0; i < vector.Length; i++)
            {
                vector[i] = (float)(vector[i] / magnitude);
            }
        }
    }
}
