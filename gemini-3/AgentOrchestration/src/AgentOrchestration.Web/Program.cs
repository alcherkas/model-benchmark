using AgentOrchestration.Web.Components;
using AgentOrchestration.Core.Agents;
using AgentOrchestration.Core.Orchestration;
using AgentOrchestration.Web.Services;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register Agents
builder.Services.AddSingleton<AnalystAgent>();
builder.Services.AddSingleton<WriterAgent>();
builder.Services.AddSingleton<EditorAgent>();
builder.Services.AddSingleton<SequentialPipeline>();

// Register Orchestration Service
builder.Services.AddScoped<IOrchestrationService, OrchestrationService>();

// Register AI Client
var provider = builder.Configuration["AgentOrchestration:Provider"];

if (provider == "Ollama")
{
    var endpoint = builder.Configuration["AgentOrchestration:Ollama:Endpoint"] ?? "http://localhost:11434";
    var model = builder.Configuration["AgentOrchestration:Ollama:Model"] ?? "llama3";
    
    // Use OllamaChatClient
    builder.Services.AddChatClient(new OllamaChatClient(new Uri(endpoint), model));
}
else if (provider == "OpenAI")
{
    var openAiKey = builder.Configuration["AgentOrchestration:OpenAI:ApiKey"];
    var openAiModel = builder.Configuration["AgentOrchestration:OpenAI:Model"] ?? "gpt-4";
    
    if (!string.IsNullOrEmpty(openAiKey) && openAiKey != "your-openai-api-key")
    {
        // Note: OpenAIChatClient is currently inaccessible in the preview package.
        // Use MockChatClient or fix the reference when stable.
        // builder.Services.AddChatClient(new OpenAIChatClient(new OpenAIClient(openAiKey), openAiModel));
        builder.Services.AddChatClient(new MockChatClient());
    }
    else
    {
        builder.Services.AddChatClient(new MockChatClient());
    }
}
else
{
    // Default to Mock for other providers or if not configured
    builder.Services.AddChatClient(new MockChatClient());
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
