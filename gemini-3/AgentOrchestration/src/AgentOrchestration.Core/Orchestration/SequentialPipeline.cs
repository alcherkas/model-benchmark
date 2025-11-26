using System.Collections.Generic;
using System.Linq;
using AgentOrchestration.Core.Agents;

namespace AgentOrchestration.Core.Orchestration;

public class SequentialPipeline
{
    public IReadOnlyList<IAgentDefinition> Agents { get; }

    public SequentialPipeline(
        AnalystAgent analyst,
        WriterAgent writer,
        EditorAgent editor)
    {
        Agents = new List<IAgentDefinition> { analyst, writer, editor };
    }
}
