using AgentOrchestration.Core.Models;

namespace AgentOrchestration.Core.Orchestration;

/// <summary>
/// Defines the contract for the orchestration service.
/// </summary>
public interface IOrchestrationService
{
    /// <summary>
    /// Executes the sequential pipeline with the given input.
    /// </summary>
    /// <param name="input">The user input to process through the pipeline.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The complete pipeline result.</returns>
    Task<PipelineResult> ExecuteAsync(
        string input,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the pipeline with streaming progress updates.
    /// Each step result is yielded as it completes.
    /// </summary>
    /// <param name="input">The user input to process through the pipeline.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>An async enumerable of step results.</returns>
    IAsyncEnumerable<StepResult> ExecuteStreamingAsync(
        string input,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current pipeline state.
    /// </summary>
    /// <returns>The current state of the pipeline.</returns>
    PipelineState GetCurrentState();

    /// <summary>
    /// Cancels the current execution if one is in progress.
    /// </summary>
    Task CancelAsync();

    /// <summary>
    /// Event raised when the pipeline state changes.
    /// </summary>
    event EventHandler<PipelineState>? StateChanged;
}
