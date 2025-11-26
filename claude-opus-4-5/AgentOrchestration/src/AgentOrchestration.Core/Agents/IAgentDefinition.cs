namespace AgentOrchestration.Core.Agents;

/// <summary>
/// Defines the contract for an AI agent in the pipeline.
/// </summary>
public interface IAgentDefinition
{
    /// <summary>
    /// Gets the unique name of the agent.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets a description of the agent's role.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the system instructions for the agent.
    /// </summary>
    string Instructions { get; }

    /// <summary>
    /// Gets the icon class for UI display (e.g., Bootstrap icon class).
    /// </summary>
    string IconClass { get; }

    /// <summary>
    /// Gets the display order in the pipeline.
    /// </summary>
    int Order { get; }
}
