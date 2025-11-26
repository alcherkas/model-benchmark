using System;
using System.Collections.Generic;

namespace AgentOrchestration.Core.Models;

public record PipelineResult(
    bool Success,
    string FinalOutput,
    IReadOnlyList<StepResult> Steps,
    TimeSpan TotalDuration,
    string? Error = null);
