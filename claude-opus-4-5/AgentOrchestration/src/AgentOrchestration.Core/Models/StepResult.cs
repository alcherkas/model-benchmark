namespace AgentOrchestration.Core.Models;

/// <summary>
/// Represents the result of a single agent step in the pipeline.
/// </summary>
/// <param name="AgentName">The name of the agent that executed this step.</param>
/// <param name="Status">The execution status of the step.</param>
/// <param name="Output">The output produced by the agent, if any.</param>
/// <param name="Duration">The time taken to execute this step.</param>
/// <param name="TokensUsed">The number of tokens consumed during execution.</param>
/// <param name="Error">Error message if the step failed.</param>
public record StepResult(
    string AgentName,
    AgentStatus Status,
    string? Output,
    TimeSpan Duration,
    int TokensUsed = 0,
    string? Error = null)
{
    /// <summary>
    /// Creates a pending step result for an agent.
    /// </summary>
    public static StepResult Pending(string agentName) =>
        new(agentName, AgentStatus.Pending, null, TimeSpan.Zero);

    /// <summary>
    /// Creates a running step result for an agent.
    /// </summary>
    public static StepResult Running(string agentName) =>
        new(agentName, AgentStatus.Running, null, TimeSpan.Zero);

    /// <summary>
    /// Creates a completed step result with output.
    /// </summary>
    public static StepResult Completed(string agentName, string output, TimeSpan duration, int tokens = 0) =>
        new(agentName, AgentStatus.Completed, output, duration, tokens);

    /// <summary>
    /// Creates a failed step result with error.
    /// </summary>
    public static StepResult Failed(string agentName, string error, TimeSpan duration) =>
        new(agentName, AgentStatus.Failed, null, duration, Error: error);

    /// <summary>
    /// Creates a cancelled step result.
    /// </summary>
    public static StepResult Cancelled(string agentName) =>
        new(agentName, AgentStatus.Cancelled, null, TimeSpan.Zero);
}
