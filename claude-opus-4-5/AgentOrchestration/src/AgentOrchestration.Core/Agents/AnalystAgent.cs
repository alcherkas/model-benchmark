namespace AgentOrchestration.Core.Agents;

/// <summary>
/// The Analyst agent performs market research and audience analysis.
/// This is the first stage in the content generation pipeline.
/// </summary>
public class AnalystAgent : IAgentDefinition
{
    /// <inheritdoc />
    public string Name => "Analyst";

    /// <inheritdoc />
    public string Description => "Market research & audience analysis";

    /// <inheritdoc />
    public string IconClass => "bi-graph-up";

    /// <inheritdoc />
    public int Order => 1;

    /// <inheritdoc />
    public string Instructions => """
        You are a marketing analyst with expertise in market research and consumer behavior.
        
        Your task is to analyze the provided product or service and deliver a comprehensive analysis that includes:

        1. **Target Audience Analysis**
           - Demographics (age, gender, income level, education, location)
           - Psychographics (lifestyle, values, interests, pain points)
           - Buying behavior and decision-making factors

        2. **Key Value Propositions**
           - Primary benefits and features
           - Unique selling points (USPs)
           - Problem-solution alignment

        3. **Competitive Positioning**
           - Market positioning strategy
           - Differentiation factors
           - Competitive advantages

        4. **Messaging Recommendations**
           - Recommended tone of voice (professional, casual, aspirational, etc.)
           - Key messaging themes
           - Emotional triggers to leverage

        Format your analysis with clear sections and bullet points for easy consumption by the content writer.
        Be specific and actionable in your recommendations.
        """;
}
