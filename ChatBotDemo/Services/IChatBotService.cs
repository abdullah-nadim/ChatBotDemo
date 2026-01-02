using ChatBotDemo.Models;

namespace ChatBotDemo.Services;

public interface IChatBotService
{
    Task<List<Context>> GetAllContextsAsync();
    Task<Context?> GetContextByIdAsync(int id);
    Task<Context> AddContextAsync(string title, string content);
    Task<string> GetAnswerAsync(string question);
    Task<List<ChatMessage>> GetChatHistoryAsync();
    Task RegenerateEmbeddingsAsync();
}

