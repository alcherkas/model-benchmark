using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AgentOrchestration.Core.Agents;
using AgentOrchestration.Core.Models;
using Microsoft.Extensions.AI;

namespace AgentOrchestration.Core.Orchestration;

public class OrchestrationService : IOrchestrationService
{
    private readonly SequentialPipeline _pipeline;
    private readonly IChatClient _chatClient;
    private PipelineState _currentState;
    private CancellationTokenSource? _cts;

    public OrchestrationService(SequentialPipeline pipeline, IChatClient chatClient)
    {
        _pipeline = pipeline;
        _chatClient = chatClient;
        _currentState = new PipelineState(false, -1, new List<StepResult>(), null);
    }

    public async Task<PipelineResult> ExecuteAsync(string input, CancellationToken cancellationToken = default)
    {
        var steps = new List<StepResult>();
        var currentInput = input;
        var startTime = DateTime.UtcNow;

        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var token = _cts.Token;

        try
        {
            _currentState = _currentState with { IsRunning = true, CurrentStepIndex = 0 };

            foreach (var (agent, index) in _pipeline.Agents.Select((a, i) => (a, i)))
            {
                var stepStart = DateTime.UtcNow;
                _currentState = _currentState with { CurrentStepIndex = index, CurrentStep = new StepResult(agent.Name, AgentStatus.Running, null, TimeSpan.Zero) };

                var messages = new List<ChatMessage>
                {
                    new ChatMessage(ChatRole.System, agent.Instructions),
                    new ChatMessage(ChatRole.User, currentInput)
                };

                var response = await _chatClient.GetResponseAsync(messages, null, token);
                var output = response.Text ?? string.Empty;
                var duration = DateTime.UtcNow - stepStart;

                var stepResult = new StepResult(agent.Name, AgentStatus.Completed, output, duration);
                steps.Add(stepResult);
                currentInput = output;

                _currentState = _currentState with { 
                    CompletedSteps = steps.ToList().AsReadOnly(), 
                    CurrentStep = null 
                };
            }

            return new PipelineResult(true, currentInput, steps, DateTime.UtcNow - startTime);
        }
        catch (Exception ex)
        {
            return new PipelineResult(false, string.Empty, steps, DateTime.UtcNow - startTime, ex.Message);
        }
        finally
        {
            _currentState = _currentState with { IsRunning = false };
        }
    }

    public async IAsyncEnumerable<StepResult> ExecuteStreamingAsync(string input, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var steps = new List<StepResult>();
        var currentInput = input;
        
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var token = _cts.Token;
        
        _currentState = _currentState with { IsRunning = true, CurrentStepIndex = 0 };

        foreach (var (agent, index) in _pipeline.Agents.Select((a, i) => (a, i)))
        {
            var stepStart = DateTime.UtcNow;
            _currentState = _currentState with { CurrentStepIndex = index, CurrentStep = new StepResult(agent.Name, AgentStatus.Running, null, TimeSpan.Zero) };
            
            yield return new StepResult(agent.Name, AgentStatus.Running, null, TimeSpan.Zero);

            var messages = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.System, agent.Instructions),
                new ChatMessage(ChatRole.User, currentInput)
            };

            var sb = new StringBuilder();
            
            await foreach (var update in _chatClient.GetStreamingResponseAsync(messages, null, token))
            {
                if (update.Text != null)
                {
                    sb.Append(update.Text);
                }
            }
            
            var output = sb.ToString();
            var duration = DateTime.UtcNow - stepStart;
            var stepResult = new StepResult(agent.Name, AgentStatus.Completed, output, duration);
            
            steps.Add(stepResult);
            currentInput = output;
            
            _currentState = _currentState with { 
                CompletedSteps = steps.ToList().AsReadOnly(), 
                CurrentStep = null 
            };
            
            yield return stepResult;
        }
        
        _currentState = _currentState with { IsRunning = false };
    }

    public PipelineState GetCurrentState() => _currentState;

    public Task CancelAsync()
    {
        _cts?.Cancel();
        return Task.CompletedTask;
    }
}
