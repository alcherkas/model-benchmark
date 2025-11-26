using System;

namespace AgentOrchestration.Core.Models;

public record StepResult(
    string AgentName,
    AgentStatus Status,
    string? Output,
    TimeSpan Duration,
    int TokensUsed = 0);
