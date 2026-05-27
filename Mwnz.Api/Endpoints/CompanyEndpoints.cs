using Mwnz.Api.Services;

namespace Mwnz.Api.Endpoints;

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
        CancellationToken cancellationToken)
    {
        var result = await companyService.GetCompanyAsync(id, cancellationToken);

        return result.Kind switch
        {
            CompanyResultKind.Success => Results.Ok(result.Company),
            CompanyResultKind.NotFound => Results.NotFound(result.Error),
            CompanyResultKind.UpstreamError or CompanyResultKind.InvalidResponse =>
                Results.Json(result.Error, statusCode: StatusCodes.Status502BadGateway),
            _ => Results.Json(result.Error, statusCode: StatusCodes.Status502BadGateway)
        };
    }
}
