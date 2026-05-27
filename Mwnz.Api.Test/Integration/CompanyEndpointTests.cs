using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Mwnz.Api.Models;
using Mwnz.Api.Services;
using Mwnz.Api.Test;

namespace Mwnz.Api.Test.Integration;

[Trait("Category", TestCategories.Integration)]
public class CompanyEndpointTests : IClassFixture<MwnzApiWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly FakeXmlCompanyClient _fakeXmlClient;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public CompanyEndpointTests(MwnzApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _fakeXmlClient = factory.FakeXmlClient;
    }

    [Fact]
    public async Task GetCompany_ReturnsJsonMatchingOpenApiSpec()
    {
        _fakeXmlClient.FetchHandler = _ => new XmlFetchResult(
            XmlFetchStatus.Success,
            """
            <Data>
              <id>1</id>
              <name>MWNZ</name>
              <description>..is awesome</description>
            </Data>
            """);

        var response = await _client.GetAsync("/v1/companies/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var company = await response.Content.ReadFromJsonAsync<Company>(JsonOptions);
        Assert.NotNull(company);
        Assert.Equal(1, company.Id);
        Assert.Equal("MWNZ", company.Name);
        Assert.Equal("..is awesome", company.Description);
    }

    [Fact]
    public async Task GetCompany_WhenXmlServiceReturnsNotFound_Returns404WithErrorBody()
    {
        _fakeXmlClient.FetchHandler = _ => new XmlFetchResult(XmlFetchStatus.NotFound);

        var response = await _client.GetAsync("/v1/companies/404");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ApiError>(JsonOptions);
        Assert.NotNull(error);
        Assert.Equal("not_found", error.Error);
        Assert.False(string.IsNullOrWhiteSpace(error.ErrorDescription));
    }

    [Fact]
    public async Task GetCompany_WhenXmlServiceFails_Returns502WithErrorBody()
    {
        _fakeXmlClient.FetchHandler = _ => new XmlFetchResult(XmlFetchStatus.UpstreamError);

        var response = await _client.GetAsync("/v1/companies/1");

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ApiError>(JsonOptions);
        Assert.NotNull(error);
        Assert.Equal("upstream_error", error.Error);
    }

    [Fact]
    public async Task Health_ReturnsOk()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
