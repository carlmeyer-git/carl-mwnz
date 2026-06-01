using Microsoft.Extensions.Logging;
using Mwnz.Api.Integrations.XmlCompany;
using Mwnz.Api.Models;

namespace Mwnz.Api.Services;

public sealed class CompanyService(
    IXmlCompanyClient xmlClient,
    IXmlCompanyParser parser,
    ILogger<CompanyService> logger) : ICompanyService
{
    private static readonly ApiError NotFoundError = new("not_found", "Company not found");
    private static readonly ApiError UpstreamError = new("upstream_error", "Unable to retrieve company from XML service");
    private static readonly ApiError InvalidResponseError = new("invalid_response", "XML service returned an invalid response");

    public async Task<CompanyResult> GetCompanyAsync(int companyId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Loading company {CompanyId}", companyId);

        var fetch = await xmlClient.FetchCompanyXmlAsync(companyId, cancellationToken);

        var result = fetch.Status switch
        {
            XmlFetchStatus.NotFound => new CompanyResult(CompanyResultKind.NotFound, Error: NotFoundError),
            XmlFetchStatus.UpstreamError => new CompanyResult(CompanyResultKind.UpstreamError, Error: UpstreamError),
            XmlFetchStatus.Success when parser.TryParse(fetch.XmlContent!, out var company) =>
                new CompanyResult(CompanyResultKind.Success, Company: company),
            // HTTP succeeded but XML did not match the expected shape.
            _ => new CompanyResult(CompanyResultKind.InvalidResponse, Error: InvalidResponseError)
        };

        var level = result.Kind switch
        {
            CompanyResultKind.Success => LogLevel.Debug,
            CompanyResultKind.NotFound => LogLevel.Information,
            _ => LogLevel.Warning
        };
        logger.Log(level, "Company {CompanyId} resolved as {ResultKind}", companyId, result.Kind);

        return result;
    }
}
