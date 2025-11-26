namespace AgentOrchestration.Core.Models;

/// <summary>
/// Configuration options for the orchestration service.
/// </summary>
public class OrchestrationOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "AgentOrchestration";

    /// <summary>
    /// The AI provider to use (AzureOpenAI, OpenAI, or Ollama).
    /// </summary>
    public string Provider { get; set; } = "AzureOpenAI";

    /// <summary>
    /// Azure OpenAI configuration.
    /// </summary>
    public AzureOpenAIOptions AzureOpenAI { get; set; } = new();

    /// <summary>
    /// OpenAI configuration.
    /// </summary>
    public OpenAIOptions OpenAI { get; set; } = new();

    /// <summary>
    /// Ollama configuration for local LLM.
    /// </summary>
    public OllamaOptions Ollama { get; set; } = new();

    /// <summary>
    /// Pipeline execution configuration.
    /// </summary>
    public PipelineOptions Pipeline { get; set; } = new();
}

/// <summary>
/// Azure OpenAI specific configuration.
/// </summary>
public class AzureOpenAIOptions
{
    /// <summary>
    /// The Azure OpenAI endpoint URL.
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// The deployment name for the model.
    /// </summary>
    public string DeploymentName { get; set; } = "gpt-4";

    /// <summary>
    /// The API key for authentication.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
}

/// <summary>
/// OpenAI specific configuration.
/// </summary>
public class OpenAIOptions
{
    /// <summary>
    /// The OpenAI API key.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// The model to use.
    /// </summary>
    public string Model { get; set; } = "gpt-4";
}

/// <summary>
/// Ollama configuration for local LLM inference.
/// </summary>
public class OllamaOptions
{
    /// <summary>
    /// The Ollama server endpoint URL.
    /// </summary>
    public string Endpoint { get; set; } = "http://localhost:11434/v1";

    /// <summary>
    /// The model to use (e.g., llama3.2, mistral, codellama, etc.).
    /// </summary>
    public string Model { get; set; } = "llama3.2";
}

/// <summary>
/// Pipeline execution configuration.
/// </summary>
public class PipelineOptions
{
    /// <summary>
    /// Maximum time in seconds for the entire pipeline execution.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 120;

    /// <summary>
    /// Whether to enable streaming responses.
    /// </summary>
    public bool EnableStreaming { get; set; } = true;

    /// <summary>
    /// Maximum tokens per agent response.
    /// </summary>
    public int MaxTokensPerAgent { get; set; } = 2000;
}
