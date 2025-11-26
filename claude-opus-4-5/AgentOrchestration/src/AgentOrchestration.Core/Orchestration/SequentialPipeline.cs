using AgentOrchestration.Core.Agents;

namespace AgentOrchestration.Core.Orchestration;

/// <summary>
/// Configures the sequential pipeline with ordered agent definitions.
/// </summary>
public class SequentialPipeline
{
    private readonly List<IAgentDefinition> _agents = [];

    /// <summary>
    /// Gets the ordered list of agents in the pipeline.
    /// </summary>
    public IReadOnlyList<IAgentDefinition> Agents => _agents.AsReadOnly();

    /// <summary>
    /// Gets the number of agents in the pipeline.
    /// </summary>
    public int StepCount => _agents.Count;

    /// <summary>
    /// Creates a new sequential pipeline with the default content generation agents.
    /// </summary>
    public static SequentialPipeline CreateContentPipeline()
    {
        var pipeline = new SequentialPipeline();
        pipeline.AddAgent(new AnalystAgent());
        pipeline.AddAgent(new WriterAgent());
        pipeline.AddAgent(new EditorAgent());
        return pipeline;
    }

    /// <summary>
    /// Adds an agent to the pipeline in order.
    /// </summary>
    /// <param name="agent">The agent definition to add.</param>
    /// <returns>This pipeline for method chaining.</returns>
    public SequentialPipeline AddAgent(IAgentDefinition agent)
    {
        _agents.Add(agent);
        // Sort by order to maintain proper sequence
        _agents.Sort((a, b) => a.Order.CompareTo(b.Order));
        return this;
    }

    /// <summary>
    /// Removes all agents from the pipeline.
    /// </summary>
    public void Clear() => _agents.Clear();

    /// <summary>
    /// Gets the agent at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the agent.</param>
    /// <returns>The agent definition at the specified index.</returns>
    public IAgentDefinition GetAgent(int index) => _agents[index];

    /// <summary>
    /// Gets the index of the specified agent by name.
    /// </summary>
    /// <param name="agentName">The name of the agent.</param>
    /// <returns>The index of the agent, or -1 if not found.</returns>
    public int GetAgentIndex(string agentName) =>
        _agents.FindIndex(a => a.Name.Equals(agentName, StringComparison.OrdinalIgnoreCase));
}
