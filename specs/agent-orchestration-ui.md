# Agent Orchestration UI Specification

## Overview

A Blazor Server application demonstrating **Sequential Orchestration** using the **Microsoft Agent Framework** (`Microsoft.Agents.AI.*` packages). The UI provides real-time visualization of a multi-agent pipeline where specialized AI agents process tasks in a linear sequence.

## Architecture

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         Agent Orchestration UI                               │
├─────────────────────────────────────────────────────────────────────────────┤
│  PRESENTATION LAYER (Blazor Server)                                          │
│  ├── Pages/                                                                  │
│  │   ├── Index.razor          - Dashboard with pipeline visualization       │
│  │   └── History.razor        - Past orchestration runs                     │
│  ├── Components/                                                             │
│  │   ├── PipelineView.razor   - Visual pipeline stages                      │
│  │   ├── AgentCard.razor      - Individual agent status card                │
│  │   ├── InputPanel.razor     - User input form                             │
│  │   └── OutputViewer.razor   - Real-time streaming output                  │
│  └── Services/                                                               │
│      └── OrchestrationHubService.cs - SignalR real-time updates             │
├─────────────────────────────────────────────────────────────────────────────┤
│  ORCHESTRATION LAYER (AgentOrchestration.Core)                               │
│  ├── Orchestration/                                                          │
│  │   ├── SequentialPipeline.cs      - Pipeline configuration                │
│  │   ├── OrchestrationService.cs    - Main orchestration service            │
│  │   └── PipelineResult.cs          - Result model with intermediate steps  │
│  ├── Agents/                                                                 │
│  │   ├── IAgentDefinition.cs        - Agent contract                        │
│  │   ├── AnalystAgent.cs            - Stage 1: Analysis                     │
│  │   ├── WriterAgent.cs             - Stage 2: Content generation           │
│  │   └── EditorAgent.cs             - Stage 3: Review and polish            │
│  └── Models/                                                                 │
│      ├── AgentStatus.cs             - Agent execution state                 │
│      ├── PipelineState.cs           - Overall pipeline state                │
│      └── StepResult.cs              - Individual step output                │
├─────────────────────────────────────────────────────────────────────────────┤
│  AI SERVICES LAYER                                                           │
│  ├── Microsoft.Agents.AI            - Core agent framework                  │
│  ├── Microsoft.Agents.AI.Workflows  - Sequential orchestration patterns     │
│  └── Microsoft.Agents.AI.OpenAI     - OpenAI/Azure OpenAI connector         │
└─────────────────────────────────────────────────────────────────────────────┘
```

## Technology Stack

| Component | Technology | Version |
|-----------|------------|---------|
| **UI Framework** | Blazor Server (Interactive SSR) | .NET 10.0 |
| **Real-time** | SignalR (built into Blazor Server) | - |
| **Agent Framework** | Microsoft.Agents.AI.* | 1.0.0-preview |
| **AI Provider** | Azure OpenAI / OpenAI | Configurable |
| **Styling** | Bootstrap 5 / Custom CSS | 5.3 |

## NuGet Packages

```xml
<!-- Core Agent Framework -->
<PackageReference Include="Microsoft.Agents.AI" Version="1.0.0-preview.*" />
<PackageReference Include="Microsoft.Agents.AI.Abstractions" Version="1.0.0-preview.*" />
<PackageReference Include="Microsoft.Agents.AI.Workflows" Version="1.0.0-preview.*" />
<PackageReference Include="Microsoft.Agents.AI.OpenAI" Version="1.0.0-preview.*" />

<!-- Logging & Observability -->
<PackageReference Include="Microsoft.Extensions.Logging" Version="10.0.*" />
<PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.11.*" />
```

## Sequential Orchestration Pattern

### Concept (from Microsoft Architecture Guidance)

Sequential orchestration **chains AI agents in a predefined, linear order**. Each agent processes the output from the previous agent, creating a **pipeline of specialized transformations**.

```
Input → Agent 1 (Analyst) → Agent 2 (Writer) → Agent 3 (Editor) → Final Output
              ↓                   ↓                  ↓
         Analysis            Draft Content      Polished Content
```

### Key Characteristics

- **Deterministic flow**: The sequence of agents is predefined, not decided dynamically
- **Progressive refinement**: Each stage builds on and improves the previous output
- **Shared state**: Agents can access common state throughout the pipeline
- **Specialized roles**: Each agent has focused instructions and capabilities

### When to Use

✅ Multi-stage processes with clear linear dependencies  
✅ Data transformation pipelines where each stage adds specific value  
✅ Workflow stages that cannot be parallelized  
✅ Progressive refinement requirements (draft → review → polish)  

### When to Avoid

❌ Stages that can run in parallel (use concurrent orchestration)  
❌ Dynamic routing based on intermediate results (use router pattern)  
❌ Workflows requiring backtracking or iteration  

## Sample Use Case: Content Generation Pipeline

### Scenario

Generate marketing content for a product through a 3-stage pipeline:

| Stage | Agent | Role | Input | Output |
|-------|-------|------|-------|--------|
| 1 | **Analyst** | Market research & audience analysis | Product description | Target audience profile, key selling points |
| 2 | **Writer** | Content creation | Analysis + Original request | Draft marketing copy |
| 3 | **Editor** | Review & polish | Draft content | Final polished content |

### Agent Definitions

```csharp
// Stage 1: Analyst Agent
public class AnalystAgent : IAgentDefinition
{
    public string Name => "Analyst";
    public string Instructions => """
        You are a marketing analyst. Analyze the product and identify:
        1. Target audience demographics and psychographics
        2. Key value propositions and selling points
        3. Competitive advantages
        4. Recommended tone and messaging approach
        
        Provide a structured analysis that will guide content creation.
        """;
}

// Stage 2: Writer Agent
public class WriterAgent : IAgentDefinition
{
    public string Name => "Writer";
    public string Instructions => """
        You are a marketing copywriter. Using the analysis provided:
        1. Create compelling headlines and taglines
        2. Write engaging product descriptions
        3. Develop call-to-action phrases
        4. Ensure messaging aligns with target audience
        
        Produce draft marketing content ready for editorial review.
        """;
}

// Stage 3: Editor Agent
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
```

## UI Components

### 1. Pipeline Visualization

```
┌─────────────────────────────────────────────────────────────────┐
│                    Sequential Pipeline                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│   ┌──────────┐      ┌──────────┐      ┌──────────┐            │
│   │ Analyst  │ ───► │  Writer  │ ───► │  Editor  │            │
│   │  ✓ Done  │      │ Running  │      │ Pending  │            │
│   │  2.3s    │      │  1.5s... │      │    -     │            │
│   └──────────┘      └──────────┘      └──────────┘            │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### 2. Agent Status Card

Each agent card displays:
- Agent name and role icon
- Current status (Pending | Running | Completed | Error)
- Execution time
- Token usage (optional)
- Expandable output preview

### 3. Input Panel

- Text area for user input (product description)
- Optional parameters (tone, length, format)
- "Run Pipeline" button
- Cancel/Reset controls

### 4. Output Viewer

- Real-time streaming output display
- Tab view for each stage's output
- Final combined result view
- Copy/Export functionality

## Configuration

### appsettings.json

```json
{
  "AgentOrchestration": {
    "Provider": "AzureOpenAI",
    "AzureOpenAI": {
      "Endpoint": "https://your-resource.openai.azure.com/",
      "DeploymentName": "gpt-4",
      "ApiKey": "your-api-key"
    },
    "OpenAI": {
      "ApiKey": "your-openai-api-key",
      "Model": "gpt-4"
    },
    "Pipeline": {
      "TimeoutSeconds": 120,
      "EnableStreaming": true,
      "MaxTokensPerAgent": 2000
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.Agents": "Debug"
    }
  }
}
```

## Project Structure

```
AgentOrchestration/
├── AgentOrchestration.sln
├── src/
│   ├── AgentOrchestration.Web/           # Blazor Server UI
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   ├── Pages/
│   │   │   ├── Index.razor
│   │   │   ├── Index.razor.cs
│   │   │   └── History.razor
│   │   ├── Components/
│   │   │   ├── PipelineView.razor
│   │   │   ├── AgentCard.razor
│   │   │   ├── InputPanel.razor
│   │   │   └── OutputViewer.razor
│   │   ├── Services/
│   │   │   └── OrchestrationHubService.cs
│   │   └── wwwroot/
│   │       └── css/
│   │           └── app.css
│   │
│   └── AgentOrchestration.Core/          # Agent orchestration logic
│       ├── Orchestration/
│       │   ├── SequentialPipeline.cs
│       │   ├── OrchestrationService.cs
│       │   ├── IOrchestrationService.cs
│       │   └── PipelineResult.cs
│       ├── Agents/
│       │   ├── IAgentDefinition.cs
│       │   ├── AnalystAgent.cs
│       │   ├── WriterAgent.cs
│       │   └── EditorAgent.cs
│       ├── Models/
│       │   ├── AgentStatus.cs
│       │   ├── PipelineState.cs
│       │   ├── StepResult.cs
│       │   └── OrchestrationOptions.cs
│       └── Extensions/
│           └── ServiceCollectionExtensions.cs
│
└── tests/
    └── AgentOrchestration.Tests/
        ├── OrchestrationServiceTests.cs
        └── PipelineTests.cs
```

## Implementation Steps

### Phase 1: Project Setup
1. Create solution with Web and Core projects
2. Add NuGet package references
3. Configure basic Blazor Server app
4. Set up configuration and DI

### Phase 2: Core Orchestration
1. Define agent interfaces and models
2. Implement agent definitions (Analyst, Writer, Editor)
3. Create SequentialPipeline orchestration class
4. Build OrchestrationService with streaming support

### Phase 3: UI Implementation
1. Create PipelineView component with visual stages
2. Build AgentCard component with status display
3. Implement InputPanel with form validation
4. Create OutputViewer with real-time streaming

### Phase 4: Integration
1. Wire up SignalR for real-time updates
2. Connect UI to orchestration service
3. Add error handling and retry logic
4. Implement logging and observability

### Phase 5: Polish
1. Add loading states and animations
2. Implement history/persistence (optional)
3. Add export functionality
4. Write tests

## API Contract

### IOrchestrationService

```csharp
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
```

### Models

```csharp
public record PipelineResult(
    bool Success,
    string FinalOutput,
    IReadOnlyList<StepResult> Steps,
    TimeSpan TotalDuration,
    string? Error = null);

public record StepResult(
    string AgentName,
    AgentStatus Status,
    string? Output,
    TimeSpan Duration,
    int TokensUsed = 0);

public enum AgentStatus
{
    Pending,
    Running,
    Completed,
    Failed,
    Cancelled
}

public record PipelineState(
    bool IsRunning,
    int CurrentStepIndex,
    IReadOnlyList<StepResult> CompletedSteps,
    StepResult? CurrentStep);
```

## Future Enhancements

1. **Additional Orchestration Patterns**
   - Concurrent orchestration for parallel stages
   - Conditional branching based on intermediate results
   - Human-in-the-loop approval gates

2. **Enhanced UI**
   - Drag-and-drop pipeline builder
   - Custom agent configuration
   - Visual workflow designer

3. **Persistence**
   - Save/load pipeline configurations
   - Execution history with replay
   - Template library

4. **Observability**
   - OpenTelemetry distributed tracing
   - Cost tracking per execution
   - Performance analytics dashboard
