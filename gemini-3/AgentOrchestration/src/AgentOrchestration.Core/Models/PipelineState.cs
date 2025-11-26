using System.Collections.Generic;

namespace AgentOrchestration.Core.Models;

public record PipelineState(
    bool IsRunning,
    int CurrentStepIndex,
    IReadOnlyList<StepResult> CompletedSteps,
    StepResult? CurrentStep);
