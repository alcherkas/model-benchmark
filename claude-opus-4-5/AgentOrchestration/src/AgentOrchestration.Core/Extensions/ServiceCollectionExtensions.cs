using AgentOrchestration.Core.Models;
using AgentOrchestration.Core.Orchestration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgentOrchestration.Core.Extensions;

/// <summary>
/// Extension methods for configuring agent orchestration services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds agent orchestration services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration instance.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddAgentOrchestration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind configuration
        var section = configuration.GetSection(OrchestrationOptions.SectionName);
        services.Configure<OrchestrationOptions>(options =>
        {
            section.Bind(options);
        });

        // Register pipeline
        services.AddSingleton(SequentialPipeline.CreateContentPipeline());

        // Register orchestration service
        services.AddScoped<IOrchestrationService, OrchestrationService>();

        return services;
    }

    /// <summary>
    /// Adds agent orchestration services with a custom pipeline configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration instance.</param>
    /// <param name="configurePipeline">Action to configure the pipeline.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddAgentOrchestration(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<SequentialPipeline> configurePipeline)
    {
        // Bind configuration
        var section = configuration.GetSection(OrchestrationOptions.SectionName);
        services.Configure<OrchestrationOptions>(options =>
        {
            section.Bind(options);
        });

        // Register custom pipeline
        var pipeline = new SequentialPipeline();
        configurePipeline(pipeline);
        services.AddSingleton(pipeline);

        // Register orchestration service
        services.AddScoped<IOrchestrationService, OrchestrationService>();

        return services;
    }
}
