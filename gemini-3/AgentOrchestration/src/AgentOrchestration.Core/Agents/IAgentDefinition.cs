namespace AgentOrchestration.Core.Agents;

public interface IAgentDefinition
{
    string Name { get; }
    string Instructions { get; }
}
