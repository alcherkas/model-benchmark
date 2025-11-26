namespace AgentOrchestration.Core.Models;

/// <summary>
/// Represents the current state of the orchestration pipeline.
/// </summary>
/// <param name="IsRunning">Whether the pipeline is currently executing.</param>
/// <param name="CurrentStepIndex">The index of the currently executing step (0-based).</param>
/// <param name="CompletedSteps">List of completed step results.</param>
/// <param name="CurrentStep">The currently executing step, if any.</param>
/// <param name="TotalSteps">Total number of steps in the pipeline.</param>
public record PipelineState(
    bool IsRunning,
    int CurrentStepIndex,
    IReadOnlyList<StepResult> CompletedSteps,
    StepResult? CurrentStep,
    int TotalSteps)
{
    /// <summary>
    /// Creates an initial idle pipeline state.
    /// </summary>
    public static PipelineState Initial(int totalSteps) =>
        new(false, -1, Array.Empty<StepResult>(), null, totalSteps);

    /// <summary>
    /// Gets whether the pipeline has completed (successfully or with error).
    /// </summary>
    public bool IsCompleted => !IsRunning && CompletedSteps.Count == TotalSteps;

    /// <summary>
    /// Gets whether the pipeline failed.
    /// </summary>
    public bool HasFailed => CompletedSteps.Any(s => s.Status == AgentStatus.Failed);

    /// <summary>
    /// Gets the progress percentage (0-100).
    /// </summary>
    public int ProgressPercentage => TotalSteps > 0 
        ? (int)((CompletedSteps.Count / (double)TotalSteps) * 100) 
        : 0;
}
