using Mwnz.Api.Models;

namespace Mwnz.Api.Services;

public sealed class CompanyService(IXmlCompanyClient xmlClient, IXmlCompanyParser parser) : ICompanyService
{
    private static readonly ApiError NotFoundError = new("not_found", "Company not found");
    private static readonly ApiError UpstreamError = new("upstream_error", "Unable to retrieve company from XML service");
    private static readonly ApiError InvalidResponseError = new("invalid_response", "XML service returned an invalid response");

    public async Task<CompanyResult> GetCompanyAsync(int companyId, CancellationToken cancellationToken = default)
    {
        var fetch = await xmlClient.FetchCompanyXmlAsync(companyId, cancellationToken);

        return fetch.Status switch
        {
            XmlFetchStatus.NotFound => new CompanyResult(CompanyResultKind.NotFound, Error: NotFoundError),
            XmlFetchStatus.UpstreamError => new CompanyResult(CompanyResultKind.UpstreamError, Error: UpstreamError),
            XmlFetchStatus.Success when parser.TryParse(fetch.XmlContent!, out var company) =>
                new CompanyResult(CompanyResultKind.Success, Company: company),
            _ => new CompanyResult(CompanyResultKind.InvalidResponse, Error: InvalidResponseError)
        };
    }
}
