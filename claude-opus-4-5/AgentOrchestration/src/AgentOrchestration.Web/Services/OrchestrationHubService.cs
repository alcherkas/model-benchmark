using AgentOrchestration.Core.Models;
using AgentOrchestration.Core.Orchestration;
using Microsoft.AspNetCore.SignalR;

namespace AgentOrchestration.Web.Services;

/// <summary>
/// SignalR hub for broadcasting orchestration state changes to connected clients.
/// </summary>
public class OrchestrationHub : Hub
{
    /// <summary>
    /// Method called when the pipeline state changes.
    /// </summary>
    public const string StateChangedMethod = "StateChanged";

    /// <summary>
    /// Method called when a step completes.
    /// </summary>
    public const string StepCompletedMethod = "StepCompleted";

    /// <summary>
    /// Method called when the pipeline completes.
    /// </summary>
    public const string PipelineCompletedMethod = "PipelineCompleted";

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}

/// <summary>
/// Service that bridges the orchestration service with SignalR for real-time updates.
/// </summary>
public class OrchestrationHubService : IDisposable
{
    private readonly IHubContext<OrchestrationHub> _hubContext;
    private readonly IOrchestrationService _orchestrationService;
    private readonly ILogger<OrchestrationHubService> _logger;

    public OrchestrationHubService(
        IHubContext<OrchestrationHub> hubContext,
        IOrchestrationService orchestrationService,
        ILogger<OrchestrationHubService> logger)
    {
        _hubContext = hubContext;
        _orchestrationService = orchestrationService;
        _logger = logger;

        // Subscribe to state changes
        _orchestrationService.StateChanged += OnStateChanged;
    }

    private async void OnStateChanged(object? sender, PipelineState state)
    {
        try
        {
            _logger.LogDebug("Broadcasting state change: Step {Step}/{Total}, Running: {IsRunning}",
                state.CurrentStepIndex + 1, state.TotalSteps, state.IsRunning);

            await _hubContext.Clients.All.SendAsync(OrchestrationHub.StateChangedMethod, state);

            // If a step just completed, broadcast that too
            if (state.CompletedSteps.Count > 0)
            {
                var lastStep = state.CompletedSteps[^1];
                if (lastStep.Status == AgentStatus.Completed)
                {
                    await _hubContext.Clients.All.SendAsync(OrchestrationHub.StepCompletedMethod, lastStep);
                }
            }

            // Check if pipeline is complete
            if (!state.IsRunning && state.CompletedSteps.Count == state.TotalSteps)
            {
                var finalOutput = state.CompletedSteps.LastOrDefault()?.Output ?? string.Empty;
                var totalDuration = state.CompletedSteps.Aggregate(TimeSpan.Zero, (sum, s) => sum + s.Duration);
                var success = state.CompletedSteps.All(s => s.Status == AgentStatus.Completed);

                var result = success
                    ? PipelineResult.Successful(finalOutput, state.CompletedSteps, totalDuration)
                    : PipelineResult.Failed(
                        state.CompletedSteps.FirstOrDefault(s => s.Status == AgentStatus.Failed)?.Error ?? "Unknown error",
                        state.CompletedSteps,
                        totalDuration);

                await _hubContext.Clients.All.SendAsync(OrchestrationHub.PipelineCompletedMethod, result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting state change");
        }
    }

    public void Dispose()
    {
        _orchestrationService.StateChanged -= OnStateChanged;
    }
}
