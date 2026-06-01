using Microsoft.Extensions.Logging.Abstractions;
using Mwnz.Api.Integrations.XmlCompany;
using Mwnz.Api.Test;

namespace Mwnz.Api.Test.Unit;

[Trait("Category", TestCategories.Unit)]
public class XmlCompanyParserTests
{
    private readonly XmlCompanyParser _parser = new(NullLogger<XmlCompanyParser>.Instance);

    [Fact]
    public void TryParse_ValidXml_ReturnsCompany()
    {
        const string xml = """
            <Data>
              <id>1</id>
              <name>MWNZ</name>
              <description>..is awesome</description>
            </Data>
            """;

        var success = _parser.TryParse(xml, out var company);

        Assert.True(success);
        Assert.NotNull(company);
        Assert.Equal(1, company.Id);
        Assert.Equal("MWNZ", company.Name);
        Assert.Equal("..is awesome", company.Description);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("<invalid>")]
    public void TryParse_InvalidXml_ReturnsFalse(string xml)
    {
        var success = _parser.TryParse(xml, out var company);

        Assert.False(success);
        Assert.Null(company);
    }
}
