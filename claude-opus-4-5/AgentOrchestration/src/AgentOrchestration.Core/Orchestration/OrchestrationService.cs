using System.Diagnostics;
using System.Runtime.CompilerServices;
using AgentOrchestration.Core.Agents;
using AgentOrchestration.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;

namespace AgentOrchestration.Core.Orchestration;

/// <summary>
/// Orchestrates the sequential execution of AI agents in a pipeline.
/// </summary>
public class OrchestrationService : IOrchestrationService
{
    private readonly SequentialPipeline _pipeline;
    private readonly OrchestrationOptions _options;
    private readonly ILogger<OrchestrationService> _logger;
    private readonly ChatClient _chatClient;
    
    private PipelineState _currentState;
    private CancellationTokenSource? _currentCts;
    private readonly object _stateLock = new();

    /// <inheritdoc />
    public event EventHandler<PipelineState>? StateChanged;

    /// <summary>
    /// Creates a new instance of the orchestration service.
    /// </summary>
    public OrchestrationService(
        SequentialPipeline pipeline,
        IOptions<OrchestrationOptions> options,
        ILogger<OrchestrationService> logger)
    {
        _pipeline = pipeline;
        _options = options.Value;
        _logger = logger;
        _currentState = PipelineState.Initial(_pipeline.StepCount);
        
        // Initialize the OpenAI chat client based on configuration
        _chatClient = CreateChatClient();
    }

    private ChatClient CreateChatClient()
    {
        if (_options.Provider.Equals("AzureOpenAI", StringComparison.OrdinalIgnoreCase))
        {
            var endpoint = new Uri(_options.AzureOpenAI.Endpoint);
            var credential = new System.ClientModel.ApiKeyCredential(_options.AzureOpenAI.ApiKey);
            var client = new Azure.AI.OpenAI.AzureOpenAIClient(endpoint, credential);
            return client.GetChatClient(_options.AzureOpenAI.DeploymentName);
        }
        else if (_options.Provider.Equals("Ollama", StringComparison.OrdinalIgnoreCase))
        {
            // Ollama provides an OpenAI-compatible API at /v1 endpoint
            var ollamaEndpoint = new Uri(_options.Ollama.Endpoint);
            var clientOptions = new OpenAIClientOptions
            {
                Endpoint = ollamaEndpoint
            };
            // Ollama doesn't require an API key, but the SDK requires a non-empty string
            var client = new OpenAIClient(credential: new System.ClientModel.ApiKeyCredential("ollama"), options: clientOptions);
            return client.GetChatClient(_options.Ollama.Model);
        }
        else
        {
            var client = new OpenAIClient(_options.OpenAI.ApiKey);
            return client.GetChatClient(_options.OpenAI.Model);
        }
    }

    /// <inheritdoc />
    public PipelineState GetCurrentState()
    {
        lock (_stateLock)
        {
            return _currentState;
        }
    }

    /// <inheritdoc />
    public async Task CancelAsync()
    {
        _currentCts?.Cancel();
        await Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<PipelineResult> ExecuteAsync(
        string input,
        CancellationToken cancellationToken = default)
    {
        var steps = new List<StepResult>();
        var stopwatch = Stopwatch.StartNew();

        await foreach (var step in ExecuteStreamingAsync(input, cancellationToken))
        {
            steps.Add(step);
            
            if (step.Status == AgentStatus.Failed)
            {
                return PipelineResult.Failed(
                    step.Error ?? "Unknown error",
                    steps,
                    stopwatch.Elapsed);
            }
        }

        stopwatch.Stop();
        
        var finalOutput = steps.LastOrDefault()?.Output ?? string.Empty;
        return PipelineResult.Successful(finalOutput, steps, stopwatch.Elapsed);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<StepResult> ExecuteStreamingAsync(
        string input,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _currentCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var token = _currentCts.Token;
        
        var completedSteps = new List<StepResult>();
        string currentInput = input;

        // Initialize state
        UpdateState(new PipelineState(
            IsRunning: true,
            CurrentStepIndex: 0,
            CompletedSteps: completedSteps,
            CurrentStep: StepResult.Running(_pipeline.Agents[0].Name),
            TotalSteps: _pipeline.StepCount));

        _logger.LogInformation("Starting pipeline execution with {StepCount} agents", _pipeline.StepCount);

        for (int i = 0; i < _pipeline.Agents.Count; i++)
        {
            var agent = _pipeline.Agents[i];
            var stepwatch = Stopwatch.StartNew();

            // Update state to show current agent running
            UpdateState(new PipelineState(
                IsRunning: true,
                CurrentStepIndex: i,
                CompletedSteps: completedSteps.ToList(),
                CurrentStep: StepResult.Running(agent.Name),
                TotalSteps: _pipeline.StepCount));

            _logger.LogInformation("Executing agent {AgentName} (step {Step}/{Total})", 
                agent.Name, i + 1, _pipeline.StepCount);

            StepResult stepResult;
            try
            {
                token.ThrowIfCancellationRequested();

                var output = await ExecuteAgentAsync(agent, currentInput, input, token);
                stepwatch.Stop();

                stepResult = StepResult.Completed(
                    agent.Name,
                    output,
                    stepwatch.Elapsed,
                    EstimateTokens(currentInput + output));

                // Use this agent's output as input for the next agent
                currentInput = output;

                _logger.LogInformation("Agent {AgentName} completed in {Duration}ms", 
                    agent.Name, stepwatch.ElapsedMilliseconds);
            }
            catch (OperationCanceledException)
            {
                stepResult = StepResult.Cancelled(agent.Name);
                _logger.LogWarning("Agent {AgentName} was cancelled", agent.Name);
                
                completedSteps.Add(stepResult);
                UpdateState(new PipelineState(
                    IsRunning: false,
                    CurrentStepIndex: i,
                    CompletedSteps: completedSteps.ToList(),
                    CurrentStep: null,
                    TotalSteps: _pipeline.StepCount));
            }
            catch (Exception ex)
            {
                stepwatch.Stop();
                stepResult = StepResult.Failed(agent.Name, ex.Message, stepwatch.Elapsed);
                _logger.LogError(ex, "Agent {AgentName} failed with error: {Error}", 
                    agent.Name, ex.Message);
                
                completedSteps.Add(stepResult);
                UpdateState(new PipelineState(
                    IsRunning: false,
                    CurrentStepIndex: i,
                    CompletedSteps: completedSteps.ToList(),
                    CurrentStep: null,
                    TotalSteps: _pipeline.StepCount));
            }

            // Handle early termination outside catch blocks
            if (stepResult.Status == AgentStatus.Cancelled || stepResult.Status == AgentStatus.Failed)
            {
                yield return stepResult;
                yield break;
            }

            completedSteps.Add(stepResult);
            yield return stepResult;
        }

        // Final state update
        UpdateState(new PipelineState(
            IsRunning: false,
            CurrentStepIndex: _pipeline.StepCount - 1,
            CompletedSteps: completedSteps.ToList(),
            CurrentStep: null,
            TotalSteps: _pipeline.StepCount));

        _logger.LogInformation("Pipeline execution completed successfully");
    }

    private async Task<string> ExecuteAgentAsync(
        IAgentDefinition agent,
        string contextInput,
        string originalInput,
        CancellationToken cancellationToken)
    {
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(agent.Instructions)
        };

        // For the first agent, just send the original input
        // For subsequent agents, include both original context and previous output
        if (agent.Order == 1)
        {
            messages.Add(new UserChatMessage($"Please analyze the following:\n\n{originalInput}"));
        }
        else
        {
            messages.Add(new UserChatMessage(
                $"Original Request:\n{originalInput}\n\n" +
                $"Previous Stage Output:\n{contextInput}\n\n" +
                $"Please process this according to your role."));
        }

        var options = new ChatCompletionOptions
        {
            MaxOutputTokenCount = _options.Pipeline.MaxTokensPerAgent
        };

        var response = await _chatClient.CompleteChatAsync(messages, options, cancellationToken);
        
        return response.Value.Content[0].Text;
    }

    private void UpdateState(PipelineState newState)
    {
        lock (_stateLock)
        {
            _currentState = newState;
        }
        StateChanged?.Invoke(this, newState);
    }

    private static int EstimateTokens(string text)
    {
        // Rough estimation: ~4 characters per token
        return text.Length / 4;
    }
}
