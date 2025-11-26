using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;

namespace AgentOrchestration.Web.Services;

public class MockChatClient : IChatClient
{
    public ChatClientMetadata Metadata => new("Mock", new Uri("http://localhost"));

    public async Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        await Task.Delay(1000, cancellationToken); // Simulate work
        var lastMessage = messages.LastOrDefault()?.Text ?? "";
        return new ChatResponse(new[]
        {
            new ChatMessage(ChatRole.Assistant, $"[Mock Output] Processed: {lastMessage.Substring(0, Math.Min(50, lastMessage.Length))}...")
        });
    }

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var lastMessage = messages.LastOrDefault()?.Text ?? "";
        var response = $"[Mock Output] Processed: {lastMessage.Substring(0, Math.Min(50, lastMessage.Length))}...";
        
        foreach (var word in response.Split(' '))
        {
            await Task.Delay(200, cancellationToken);
            yield return new ChatResponseUpdate { Contents = new List<AIContent> { new TextContent(word + " ") } };
        }
    }
    
    public void Dispose() {}
    
    public object? GetService(Type serviceType, object? serviceKey = null) => null;
}
