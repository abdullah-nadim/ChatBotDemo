using Microsoft.AspNetCore.Mvc;
using ChatBotDemo.Models;
using ChatBotDemo.Services;

namespace ChatBotDemo.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IChatBotService _chatBotService;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IChatBotService chatBotService, ILogger<ChatController> logger)
    {
        _chatBotService = chatBotService;
        _logger = logger;
    }

    [HttpPost("ask")]
    public async Task<IActionResult> AskQuestion([FromBody] ChatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.Question))
        {
            return BadRequest(new { error = "Question is required" });
        }

        try
        {
            var answer = await _chatBotService.GetAnswerAsync(request.Question);
            return Ok(new ChatResponse { Answer = answer });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chat question");
            return StatusCode(500, new { error = "An error occurred while processing your question" });
        }
    }

    [HttpGet("contexts")]
    public async Task<IActionResult> GetContexts()
    {
        try
        {
            var contexts = await _chatBotService.GetAllContextsAsync();
            return Ok(contexts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving contexts");
            return StatusCode(500, new { error = "An error occurred while retrieving contexts" });
        }
    }

    [HttpPost("contexts")]
    public async Task<IActionResult> AddContext([FromBody] AddContextRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.Title) || string.IsNullOrWhiteSpace(request?.Content))
        {
            return BadRequest(new { error = "Title and Content are required" });
        }

        try
        {
            var context = await _chatBotService.AddContextAsync(request.Title, request.Content);
            return Ok(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding context");
            return StatusCode(500, new { error = "An error occurred while adding context" });
        }
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetChatHistory()
    {
        try
        {
            var history = await _chatBotService.GetChatHistoryAsync();
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving chat history");
            return StatusCode(500, new { error = "An error occurred while retrieving chat history" });
        }
    }

    [HttpPost("regenerate-embeddings")]
    public async Task<IActionResult> RegenerateEmbeddings()
    {
        try
        {
            await _chatBotService.RegenerateEmbeddingsAsync();
            return Ok(new { message = "Embeddings regenerated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error regenerating embeddings");
            return StatusCode(500, new { error = "An error occurred while regenerating embeddings" });
        }
    }
}

public class ChatRequest
{
    public string Question { get; set; } = string.Empty;
}

public class ChatResponse
{
    public string Answer { get; set; } = string.Empty;
}

public class AddContextRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

