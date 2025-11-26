namespace AgentOrchestration.Core.Agents;

public class WriterAgent : IAgentDefinition
{
    public string Name => "Writer";
    public string Instructions => """
        You are a marketing copywriter. Using the analysis provided:
        1. Create compelling headlines and taglines
        2. Write engaging product descriptions
        3. Develop call-to-action phrases
        4. Ensure messaging aligns with target audience
        
        Produce draft marketing content ready for editorial review.
        """;
}
