using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Moq;
using Mwnz.Api.Integrations.XmlCompany;
using Mwnz.Api.Models;
using Mwnz.Api.Test;

namespace Mwnz.Api.Test.Integration;

[Trait("Category", TestCategories.Integration)]
public class CompanyEndpointTests : IClassFixture<MwnzApiWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly Mock<IXmlCompanyClient> _xmlCompanyClientMock;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public CompanyEndpointTests(MwnzApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _xmlCompanyClientMock = factory.XmlCompanyClientMock;
        _xmlCompanyClientMock.Reset();
    }

    [Fact]
    public async Task GetCompany_ReturnsJsonMatchingOpenApiSpec()
    {
        _xmlCompanyClientMock
            .Setup(c => c.FetchCompanyXmlAsync(It.Is<int>(id => id == 1), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new XmlFetchResult(
                XmlFetchStatus.Success,
                """
                <Data>
                  <id>1</id>
                  <name>MWNZ</name>
                  <description>..is awesome</description>
                </Data>
                """));

        var response = await _client.GetAsync("/v1/companies/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var company = await response.Content.ReadFromJsonAsync<Company>(JsonOptions);
        Assert.NotNull(company);
        Assert.Equal(1, company.Id);
        Assert.Equal("MWNZ", company.Name);
        Assert.Equal("..is awesome", company.Description);

        _xmlCompanyClientMock.Verify(
            c => c.FetchCompanyXmlAsync(1, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetCompany_WhenXmlServiceReturnsNotFound_Returns404WithErrorBody()
    {
        _xmlCompanyClientMock
            .Setup(c => c.FetchCompanyXmlAsync(It.Is<int>(id => id == 404), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new XmlFetchResult(XmlFetchStatus.NotFound));

        var response = await _client.GetAsync("/v1/companies/404");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ApiError>(JsonOptions);
        Assert.NotNull(error);
        Assert.Equal("not_found", error.Error);
        Assert.False(string.IsNullOrWhiteSpace(error.ErrorDescription));

        _xmlCompanyClientMock.Verify(
            c => c.FetchCompanyXmlAsync(404, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetCompany_WhenXmlServiceFails_Returns502WithErrorBody()
    {
        _xmlCompanyClientMock
            .Setup(c => c.FetchCompanyXmlAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new XmlFetchResult(XmlFetchStatus.UpstreamError));

        var response = await _client.GetAsync("/v1/companies/1");

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ApiError>(JsonOptions);
        Assert.NotNull(error);
        Assert.Equal("upstream_error", error.Error);
    }

    [Fact]
    public async Task GetCompany_WhenUnhandledExceptionThrown_Returns500WithInternalError()
    {
        _xmlCompanyClientMock
            .Setup(c => c.FetchCompanyXmlAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Simulated failure"));

        var response = await _client.GetAsync("/v1/companies/1");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ApiError>(JsonOptions);
        Assert.NotNull(error);
        Assert.Equal("internal_error", error.Error);
    }

    [Fact]
    public async Task GetCompany_WhenXmlIsInvalid_Returns502WithInvalidResponseError()
    {
        _xmlCompanyClientMock
            .Setup(c => c.FetchCompanyXmlAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new XmlFetchResult(XmlFetchStatus.Success, "<not-valid-xml>"));

        var response = await _client.GetAsync("/v1/companies/1");

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ApiError>(JsonOptions);
        Assert.NotNull(error);
        Assert.Equal("invalid_response", error.Error);
    }

    [Fact]
    public async Task Health_ReturnsOk()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task OpenApiSpec_ReturnsYamlMatchingContract()
    {
        var response = await _client.GetAsync("/openapi/v1.yaml");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/yaml", response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("openapi: \"3.0.2\"", body);
        Assert.Contains("/companies/{id}:", body);
        Assert.Contains("MWNZ companies", body);
    }
}
