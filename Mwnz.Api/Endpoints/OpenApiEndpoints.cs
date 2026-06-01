using System.Reflection;

namespace Mwnz.Api.Endpoints;

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

    private static IResult ServeOpenApiSpec()
    {
        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(SpecResourceName);

        if (stream is null)
        {
            return Results.NotFound();
        }

        return Results.Stream(stream, "application/yaml");
    }
}
