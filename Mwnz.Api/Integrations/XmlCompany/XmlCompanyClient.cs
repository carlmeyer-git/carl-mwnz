using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mwnz.Api.Configuration;

namespace Mwnz.Api.Integrations.XmlCompany;

public sealed class XmlCompanyClient(
    HttpClient httpClient,
    IOptions<XmlApiOptions> options,
    ILogger<XmlCompanyClient> logger) : IXmlCompanyClient
{
    private readonly XmlApiOptions _options = options.Value;

    public async Task<XmlFetchResult> FetchCompanyXmlAsync(int companyId, CancellationToken cancellationToken = default)
    {
        var baseUrl = _options.BaseUrl.TrimEnd('/');
        var requestUri = $"{baseUrl}/{companyId}.xml";

        logger.LogDebug("Fetching company XML from {RequestUri}", requestUri);

        HttpResponseMessage response;
        try
        {
            response = await httpClient.GetAsync(requestUri, cancellationToken);
        }
        // HttpClient timeout surfaces as TaskCanceledException, not user cancellation.
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("Timed out fetching company {CompanyId} from {RequestUri}", companyId, requestUri);
            return new XmlFetchResult(XmlFetchStatus.UpstreamError);
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "HTTP error fetching company {CompanyId} from {RequestUri}", companyId, requestUri);
            return new XmlFetchResult(XmlFetchStatus.UpstreamError);
        }

        using (response)
        {
            // Only 404 from the XML service maps to "company not found"; other errors are upstream failures.
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                logger.LogInformation("Company {CompanyId} not found at {RequestUri}", companyId, requestUri);
                return new XmlFetchResult(XmlFetchStatus.NotFound);
            }

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Upstream returned {StatusCode} for company {CompanyId} at {RequestUri}",
                    (int)response.StatusCode,
                    companyId,
                    requestUri);
                return new XmlFetchResult(XmlFetchStatus.UpstreamError);
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogDebug("Received XML for company {CompanyId} ({ByteCount} bytes)", companyId, content.Length);
            return new XmlFetchResult(XmlFetchStatus.Success, content);
        }
    }
}
