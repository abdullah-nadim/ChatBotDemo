namespace ChatBotDemo.Models;

public class ChatMessage
{
    public int Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public int? ContextId { get; set; }
    public Context? Context { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

