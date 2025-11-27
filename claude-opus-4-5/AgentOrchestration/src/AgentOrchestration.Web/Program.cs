using AgentOrchestration.Core.Extensions;
using AgentOrchestration.Web.Components;
using AgentOrchestration.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add SignalR
builder.Services.AddSignalR();

// Add Agent Orchestration services
builder.Services.AddAgentOrchestration(builder.Configuration);

// Add OrchestrationHubService as a singleton to bridge orchestration events to SignalR
builder.Services.AddSingleton<OrchestrationHubService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

// Map SignalR hub
app.MapHub<OrchestrationHub>("/orchestrationHub");

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
