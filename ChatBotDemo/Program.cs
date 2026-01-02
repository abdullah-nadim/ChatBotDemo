using Microsoft.EntityFrameworkCore;
using ChatBotDemo.Data;
using ChatBotDemo.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add PostgreSQL connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ChatBotDbContext>(options =>
    options.UseNpgsql(connectionString, o => o.UseVector()));

// Add services
// Choose ONE embedding service:
// Option 1: MockEmbeddingService - for testing without API costs
// builder.Services.AddScoped<IEmbeddingService, MockEmbeddingService>();
// Option 2: OpenAIEmbeddingService - requires valid OpenAI API key
// builder.Services.AddScoped<IEmbeddingService, OpenAIEmbeddingService>();
// Option 3: GeminiEmbeddingService - requires valid Gemini API key
builder.Services.AddHttpClient<GeminiEmbeddingService>();
builder.Services.AddScoped<IEmbeddingService, GeminiEmbeddingService>();

// Add HttpClient for ChatBotService (for Gemini chat API)
builder.Services.AddHttpClient<ChatBotService>();
builder.Services.AddScoped<IChatBotService, ChatBotService>();

var app = builder.Build();

// Seed database and generate embeddings
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ChatBotDbContext>();
        await ChatBotDemo.Data.DbInitializer.SeedAsync(context);
        
        // Generate embeddings for contexts that don't have them
        var chatBotService = services.GetRequiredService<IChatBotService>();
        await chatBotService.RegenerateEmbeddingsAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
