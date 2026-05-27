using Mwnz.Api.Models;
using Mwnz.Api.Services;
using Mwnz.Api.Test;

namespace Mwnz.Api.Test.Unit;

[Trait("Category", TestCategories.Unit)]
public class CompanyServiceTests
{
    private const string ValidXml = """
        <Data>
          <id>2</id>
          <name>Other</name>
          <description>....is not</description>
        </Data>
        """;

    [Fact]
    public async Task GetCompanyAsync_Success_ReturnsCompany()
    {
        var client = new FakeXmlCompanyClient(new XmlFetchResult(XmlFetchStatus.Success, ValidXml));
        var service = new CompanyService(client, new XmlCompanyParser());

        var result = await service.GetCompanyAsync(2);

        Assert.Equal(CompanyResultKind.Success, result.Kind);
        Assert.NotNull(result.Company);
        Assert.Equal(2, result.Company.Id);
        Assert.Equal("Other", result.Company.Name);
    }

    [Fact]
    public async Task GetCompanyAsync_NotFound_ReturnsNotFoundError()
    {
        var client = new FakeXmlCompanyClient(new XmlFetchResult(XmlFetchStatus.NotFound));
        var service = new CompanyService(client, new XmlCompanyParser());

        var result = await service.GetCompanyAsync(99);

        Assert.Equal(CompanyResultKind.NotFound, result.Kind);
        Assert.Equal("not_found", result.Error!.Error);
    }

    [Fact]
    public async Task GetCompanyAsync_UpstreamError_ReturnsUpstreamError()
    {
        var client = new FakeXmlCompanyClient(new XmlFetchResult(XmlFetchStatus.UpstreamError));
        var service = new CompanyService(client, new XmlCompanyParser());

        var result = await service.GetCompanyAsync(1);

        Assert.Equal(CompanyResultKind.UpstreamError, result.Kind);
        Assert.Equal("upstream_error", result.Error!.Error);
    }

    [Fact]
    public async Task GetCompanyAsync_InvalidXml_ReturnsInvalidResponse()
    {
        var client = new FakeXmlCompanyClient(new XmlFetchResult(XmlFetchStatus.Success, "<bad>"));
        var service = new CompanyService(client, new XmlCompanyParser());

        var result = await service.GetCompanyAsync(1);

        Assert.Equal(CompanyResultKind.InvalidResponse, result.Kind);
        Assert.Equal("invalid_response", result.Error!.Error);
    }

    private sealed class FakeXmlCompanyClient(XmlFetchResult result) : IXmlCompanyClient
    {
        public Task<XmlFetchResult> FetchCompanyXmlAsync(int companyId, CancellationToken cancellationToken = default) =>
            Task.FromResult(result);
    }
}
