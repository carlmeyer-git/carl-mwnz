using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Mwnz.Api.Integrations.XmlCompany;
using Mwnz.Api.Configuration;
using Mwnz.Api.Test;

namespace Mwnz.Api.Test.Unit;

[Trait("Category", TestCategories.Unit)]
public class XmlCompanyClientTests
{
    private const string BaseUrl = "https://example.com/xml-api";
    private const string ValidXml = "<Data><id>1</id><name>MWNZ</name><description>test</description></Data>";

    [Fact]
    public async Task FetchCompanyXmlAsync_Success_ReturnsXmlContentAndCallsExpectedUrl()
    {
        HttpRequestMessage? capturedRequest = null;
        var handler = new StubHttpMessageHandler((request, _) =>
        {
            capturedRequest = request;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(ValidXml)
            });
        });

        var client = CreateClient(handler);

        var result = await client.FetchCompanyXmlAsync(42);

        Assert.Equal(XmlFetchStatus.Success, result.Status);
        Assert.Equal(ValidXml, result.XmlContent);
        Assert.NotNull(capturedRequest);
        Assert.Equal($"{BaseUrl}/42.xml", capturedRequest!.RequestUri?.ToString());
    }

    [Fact]
    public async Task FetchCompanyXmlAsync_NotFound_ReturnsNotFound()
    {
        var handler = new StubHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)));

        var client = CreateClient(handler);

        var result = await client.FetchCompanyXmlAsync(99);

        Assert.Equal(XmlFetchStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task FetchCompanyXmlAsync_ServerError_ReturnsUpstreamError()
    {
        var handler = new StubHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)));

        var client = CreateClient(handler);

        var result = await client.FetchCompanyXmlAsync(1);

        Assert.Equal(XmlFetchStatus.UpstreamError, result.Status);
    }

    [Fact]
    public async Task FetchCompanyXmlAsync_Timeout_ReturnsUpstreamError()
    {
        var handler = new StubHttpMessageHandler((_, cancellationToken) =>
            throw new TaskCanceledException("timed out", null, cancellationToken));

        var client = CreateClient(handler);

        var result = await client.FetchCompanyXmlAsync(1);

        Assert.Equal(XmlFetchStatus.UpstreamError, result.Status);
    }

    [Fact]
    public async Task FetchCompanyXmlAsync_HttpRequestException_ReturnsUpstreamError()
    {
        var handler = new StubHttpMessageHandler((_, _) =>
            throw new HttpRequestException("connection failed"));

        var client = CreateClient(handler);

        var result = await client.FetchCompanyXmlAsync(1);

        Assert.Equal(XmlFetchStatus.UpstreamError, result.Status);
    }

    [Fact]
    public async Task FetchCompanyXmlAsync_TrimsTrailingSlashFromBaseUrl()
    {
        HttpRequestMessage? capturedRequest = null;
        var handler = new StubHttpMessageHandler((request, _) =>
        {
            capturedRequest = request;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(ValidXml)
            });
        });

        var options = Options.Create(new XmlApiOptions { BaseUrl = $"{BaseUrl}/" });
        var httpClient = new HttpClient(handler);
        var client = new XmlCompanyClient(httpClient, options, NullLogger<XmlCompanyClient>.Instance);

        await client.FetchCompanyXmlAsync(7);

        Assert.Equal($"{BaseUrl}/7.xml", capturedRequest!.RequestUri?.ToString());
    }

    private static XmlCompanyClient CreateClient(StubHttpMessageHandler handler) =>
        new(new HttpClient(handler), Options.Create(new XmlApiOptions { BaseUrl = BaseUrl }), NullLogger<XmlCompanyClient>.Instance);

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            handler(request, cancellationToken);
    }
}
