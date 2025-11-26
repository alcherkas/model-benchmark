namespace AgentOrchestration.Core.Models;

/// <summary>
/// Represents the execution status of an agent in the pipeline.
/// </summary>
public enum AgentStatus
{
    /// <summary>
    /// Agent is waiting to be executed.
    /// </summary>
    Pending,

    /// <summary>
    /// Agent is currently executing.
    /// </summary>
    Running,

    /// <summary>
    /// Agent has successfully completed execution.
    /// </summary>
    Completed,

    /// <summary>
    /// Agent execution failed with an error.
    /// </summary>
    Failed,

    /// <summary>
    /// Agent execution was cancelled.
    /// </summary>
    Cancelled
}
