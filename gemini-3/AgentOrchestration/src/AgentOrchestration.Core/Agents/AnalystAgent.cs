namespace AgentOrchestration.Core.Agents;

public class AnalystAgent : IAgentDefinition
{
    public string Name => "Analyst";
    public string Instructions => """
        You are a marketing analyst. Analyze the product and identify:
        1. Target audience demographics and psychographics
        2. Key value propositions and selling points
        3. Competitive advantages
        4. Recommended tone and messaging approach
        
        Provide a structured analysis that will guide content creation.
        """;
}
