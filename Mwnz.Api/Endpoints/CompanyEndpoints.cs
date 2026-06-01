using Microsoft.Extensions.Logging;
using Mwnz.Api.Services;

namespace Mwnz.Api.Endpoints;

internal sealed class CompanyEndpointLogs;

public static class CompanyEndpoints
{
    public static RouteGroupBuilder MapCompanyEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/v1/companies")
            .WithTags("Companies");

        group.MapGet("/{id:int}", GetCompanyById)
            .WithName("GetCompanyById")
            .Produces<Models.Company>(StatusCodes.Status200OK)
            .Produces<Models.ApiError>(StatusCodes.Status404NotFound)
            .Produces<Models.ApiError>(StatusCodes.Status502BadGateway);

        return group;
    }

    private static async Task<IResult> GetCompanyById(
        int id,
        ICompanyService companyService,
        ILogger<CompanyEndpointLogs> logger,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GET /v1/companies/{CompanyId}", id);

        var result = await companyService.GetCompanyAsync(id, cancellationToken);

        // Upstream and parse failures both return 502; missing company returns 404.
        return result.Kind switch
        {
            CompanyResultKind.Success => LogAndReturn(
                logger,
                id,
                StatusCodes.Status200OK,
                Results.Ok(result.Company)),
            CompanyResultKind.NotFound => LogAndReturn(
                logger,
                id,
                StatusCodes.Status404NotFound,
                Results.NotFound(result.Error)),
            CompanyResultKind.UpstreamError or CompanyResultKind.InvalidResponse => LogAndReturn(
                logger,
                id,
                StatusCodes.Status502BadGateway,
                Results.Json(result.Error, statusCode: StatusCodes.Status502BadGateway)),
            _ => throw new InvalidOperationException($"Unexpected result kind: {result.Kind}")
        };
    }

    private static IResult LogAndReturn(ILogger logger, int companyId, int statusCode, IResult result)
    {
        logger.LogInformation("GET /v1/companies/{CompanyId} returned {StatusCode}", companyId, statusCode);
        return result;
    }
}
