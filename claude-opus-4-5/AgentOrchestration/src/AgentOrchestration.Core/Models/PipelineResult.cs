namespace AgentOrchestration.Core.Models;

/// <summary>
/// Represents the final result of a pipeline execution.
/// </summary>
/// <param name="Success">Whether the pipeline completed successfully.</param>
/// <param name="FinalOutput">The final output from the last agent.</param>
/// <param name="Steps">All step results from the pipeline execution.</param>
/// <param name="TotalDuration">The total time taken to execute the pipeline.</param>
/// <param name="Error">Error message if the pipeline failed.</param>
public record PipelineResult(
    bool Success,
    string FinalOutput,
    IReadOnlyList<StepResult> Steps,
    TimeSpan TotalDuration,
    string? Error = null)
{
    /// <summary>
    /// Creates a successful pipeline result.
    /// </summary>
    public static PipelineResult Successful(
        string finalOutput, 
        IReadOnlyList<StepResult> steps, 
        TimeSpan totalDuration) =>
        new(true, finalOutput, steps, totalDuration);

    /// <summary>
    /// Creates a failed pipeline result.
    /// </summary>
    public static PipelineResult Failed(
        string error, 
        IReadOnlyList<StepResult> steps, 
        TimeSpan totalDuration) =>
        new(false, string.Empty, steps, totalDuration, error);

    /// <summary>
    /// Gets the total tokens used across all steps.
    /// </summary>
    public int TotalTokensUsed => Steps.Sum(s => s.TokensUsed);

    /// <summary>
    /// Gets the output from a specific agent by name.
    /// </summary>
    public string? GetAgentOutput(string agentName) =>
        Steps.FirstOrDefault(s => s.AgentName == agentName)?.Output;
}
