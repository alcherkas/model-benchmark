# Agent Orchestration UI

A **Blazor Server** application demonstrating **Sequential Orchestration** using AI agents. The UI provides real-time visualization of a multi-agent pipeline where specialized AI agents process tasks in a linear sequence.

## 🚀 Features

- **Sequential Pipeline**: Three-stage content generation pipeline (Analyst → Writer → Editor)
- **Real-time Updates**: Live progress visualization via SignalR
- **Streaming Output**: Watch agent outputs as they're generated
- **Multiple AI Providers**: Support for Azure OpenAI, OpenAI, and **Ollama** (local LLM)
- **Modern UI**: Bootstrap 5 with custom orchestration visualizations

## 📋 Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- One of:
  - An Azure OpenAI API key
  - An OpenAI API key
  - [Ollama](https://ollama.ai) installed locally (for local LLM)

## 🛠️ Setup

### 1. Clone and Navigate

```bash
cd AgentOrchestration
```

### 2. Configure API Keys

Edit `src/AgentOrchestration.Web/appsettings.json`:

**For Azure OpenAI:**
```json
{
  "AgentOrchestration": {
    "Provider": "AzureOpenAI",
    "AzureOpenAI": {
      "Endpoint": "https://your-resource.openai.azure.com/",
      "DeploymentName": "gpt-4",
      "ApiKey": "your-api-key"
    }
  }
}
```

**For OpenAI:**
```json
{
  "AgentOrchestration": {
    "Provider": "OpenAI",
    "OpenAI": {
      "ApiKey": "your-openai-api-key",
      "Model": "gpt-4"
    }
  }
}
```

**For Ollama (Local LLM):**

1. Install Ollama from [ollama.ai](https://ollama.ai)
2. Pull a model: `ollama pull llama3.2` (or `mistral`, `codellama`, etc.)
3. Configure:
```json
{
  "AgentOrchestration": {
    "Provider": "Ollama",
    "Ollama": {
      "Endpoint": "http://localhost:11434/v1",
      "Model": "llama3.2"
    },
    "Pipeline": {
      "TimeoutSeconds": 300,
      "MaxTokensPerAgent": 2000
    }
  }
}
```

> **Note:** Local models may be slower. Increase `TimeoutSeconds` for larger models.

### 3. Run the Application

```bash
dotnet run --project src/AgentOrchestration.Web
```

### 4. Open in Browser

Navigate to `https://localhost:5001` or the URL shown in the terminal.

## 🏗️ Architecture

```
AgentOrchestration/
├── src/
│   ├── AgentOrchestration.Core/          # Core orchestration logic
│   │   ├── Agents/                       # Agent definitions
│   │   │   ├── IAgentDefinition.cs
│   │   │   ├── AnalystAgent.cs
│   │   │   ├── WriterAgent.cs
│   │   │   └── EditorAgent.cs
│   │   ├── Models/                       # Data models
│   │   │   ├── AgentStatus.cs
│   │   │   ├── PipelineState.cs
│   │   │   ├── StepResult.cs
│   │   │   ├── PipelineResult.cs
│   │   │   └── OrchestrationOptions.cs
│   │   ├── Orchestration/                # Pipeline execution
│   │   │   ├── IOrchestrationService.cs
│   │   │   ├── OrchestrationService.cs
│   │   │   └── SequentialPipeline.cs
│   │   └── Extensions/
│   │       └── ServiceCollectionExtensions.cs
│   │
│   └── AgentOrchestration.Web/           # Blazor Server UI
│       ├── Components/
│       │   ├── Orchestration/            # Pipeline UI components
│       │   │   ├── AgentCard.razor
│       │   │   ├── PipelineView.razor
│       │   │   ├── InputPanel.razor
│       │   │   └── OutputViewer.razor
│       │   └── Pages/
│       │       ├── Home.razor
│       │       └── History.razor
│       └── Services/
│           └── OrchestrationHubService.cs
```

## 🎯 How It Works

### Sequential Pipeline Pattern

The application implements a **sequential orchestration pattern** where tasks flow through a predefined series of specialized AI agents:

```
User Input → Analyst → Writer → Editor → Final Output
                ↓          ↓         ↓
           Analysis    Draft     Polished
                       Content   Content
```

### The Three Agents

1. **Analyst Agent** (Stage 1)
   - Analyzes the product/service
   - Identifies target audience
   - Determines key value propositions
   - Recommends messaging approach

2. **Writer Agent** (Stage 2)
   - Creates headlines and taglines
   - Writes product descriptions
   - Develops marketing copy variations
   - Crafts call-to-action phrases

3. **Editor Agent** (Stage 3)
   - Reviews for clarity and grammar
   - Ensures brand voice consistency
   - Optimizes for engagement
   - Produces publication-ready content

## 📦 NuGet Packages

### Core Library
- `Azure.AI.OpenAI` - Azure OpenAI SDK
- `OpenAI` - OpenAI SDK (also used for Ollama via OpenAI-compatible API)
- `Microsoft.Extensions.Logging` - Logging infrastructure

### Web Application
- `OpenTelemetry.Extensions.Hosting` - Observability

## 🔧 Configuration Options

| Option | Description | Default |
|--------|-------------|---------|
| `Provider` | AI provider (`AzureOpenAI`, `OpenAI`, or `Ollama`) | `AzureOpenAI` |
| `Ollama.Endpoint` | Ollama server URL | `http://localhost:11434/v1` |
| `Ollama.Model` | Ollama model name | `llama3.2` |
| `Pipeline.TimeoutSeconds` | Max execution time | `120` (use `300` for Ollama) |
| `Pipeline.EnableStreaming` | Stream responses | `true` |
| `Pipeline.MaxTokensPerAgent` | Token limit per agent | `2000` |

## 🧪 Development

### Build Solution
```bash
dotnet build
```

### Run Tests
```bash
dotnet test
```

### Hot Reload Development
```bash
dotnet watch --project src/AgentOrchestration.Web
```

## 📝 License

This project is provided as a demonstration of AI agent orchestration patterns.

## 🙏 Acknowledgments

- Built with [Blazor](https://blazor.net)
- AI powered by [Azure OpenAI](https://azure.microsoft.com/products/ai-services/openai-service) / [OpenAI](https://openai.com)
- UI framework: [Bootstrap 5](https://getbootstrap.com)
