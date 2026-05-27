using Microsoft.Extensions.Options;
using Mwnz.Api.Configuration;

namespace Mwnz.Api.Services;

public sealed class XmlCompanyClient(HttpClient httpClient, IOptions<XmlApiOptions> options) : IXmlCompanyClient
{
    private readonly XmlApiOptions _options = options.Value;

    public async Task<XmlFetchResult> FetchCompanyXmlAsync(int companyId, CancellationToken cancellationToken = default)
    {
        var baseUrl = _options.BaseUrl.TrimEnd('/');
        var requestUri = $"{baseUrl}/{companyId}.xml";

        HttpResponseMessage response;
        try
        {
            response = await httpClient.GetAsync(requestUri, cancellationToken);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return new XmlFetchResult(XmlFetchStatus.UpstreamError);
        }
        catch (HttpRequestException)
        {
            return new XmlFetchResult(XmlFetchStatus.UpstreamError);
        }

        using (response)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new XmlFetchResult(XmlFetchStatus.NotFound);
            }

            if (!response.IsSuccessStatusCode)
            {
                return new XmlFetchResult(XmlFetchStatus.UpstreamError);
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return new XmlFetchResult(XmlFetchStatus.Success, content);
        }
    }
}
