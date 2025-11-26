namespace AgentOrchestration.Core.Agents;

public class EditorAgent : IAgentDefinition
{
    public string Name => "Editor";
    public string Instructions => """
        You are a senior editor. Review and polish the draft content:
        1. Check for clarity, grammar, and style consistency
        2. Ensure brand voice alignment
        3. Optimize for readability and engagement
        4. Verify all claims are accurate and appropriate
        
        Produce final, publication-ready content.
        """;
}
