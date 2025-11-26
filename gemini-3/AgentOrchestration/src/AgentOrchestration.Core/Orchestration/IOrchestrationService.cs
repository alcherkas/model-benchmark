using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AgentOrchestration.Core.Models;

namespace AgentOrchestration.Core.Orchestration;

public interface IOrchestrationService
{
    /// <summary>
    /// Executes the sequential pipeline with the given input.
    /// </summary>
    Task<PipelineResult> ExecuteAsync(
        string input, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Executes the pipeline with streaming progress updates.
    /// </summary>
    IAsyncEnumerable<StepResult> ExecuteStreamingAsync(
        string input,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the current pipeline state.
    /// </summary>
    PipelineState GetCurrentState();
    
    /// <summary>
    /// Cancels the current execution.
    /// </summary>
    Task CancelAsync();
}
