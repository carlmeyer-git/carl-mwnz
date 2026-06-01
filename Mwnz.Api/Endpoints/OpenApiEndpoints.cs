using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Mwnz.Api.Endpoints;

internal sealed class OpenApiEndpointLogs;

public static class OpenApiEndpoints
{
    // Embedded at build time from openapi-companies.yaml (see Mwnz.Api.csproj).
    private const string SpecResourceName = "Mwnz.Api.openapi-companies.yaml";

    public static void MapOpenApiEndpoints(this WebApplication app)
    {
        app.MapGet("/openapi/v1.yaml", ServeOpenApiSpec)
            .WithName("GetOpenApiSpec")
            .WithTags("OpenAPI")
            .Produces<string>(StatusCodes.Status200OK, "application/yaml")
            .Produces(StatusCodes.Status404NotFound)
            .ExcludeFromDescription();
    }

    private static IResult ServeOpenApiSpec(ILogger<OpenApiEndpointLogs> logger)
    {
        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(SpecResourceName);

        if (stream is null)
        {
            logger.LogError("OpenAPI spec resource {ResourceName} was not found", SpecResourceName);
            return Results.NotFound();
        }

        logger.LogDebug("Serving OpenAPI spec from {ResourceName}", SpecResourceName);
        return Results.Stream(stream, "application/yaml");
    }
}
