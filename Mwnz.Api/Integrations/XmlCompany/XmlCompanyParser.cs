using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using Mwnz.Api.Models;
using ExternalXmlCompany = Mwnz.Api.Integrations.XmlCompany.Models.XmlCompany;

namespace Mwnz.Api.Integrations.XmlCompany;

public sealed class XmlCompanyParser(ILogger<XmlCompanyParser> logger) : IXmlCompanyParser
{
    private static readonly XmlSerializer Serializer = new(typeof(ExternalXmlCompany));

    public bool TryParse(string xml, out Company? company)
    {
        company = null;

        if (string.IsNullOrWhiteSpace(xml))
        {
            logger.LogWarning("Cannot parse company XML: content is empty");
            return false;
        }

        try
        {
            using var reader = new StringReader(xml);
            if (Serializer.Deserialize(reader) is not ExternalXmlCompany data)
            {
                logger.LogWarning("Cannot parse company XML: deserialized document was null");
                return false;
            }

            company = new Company(data.Id, data.Name, data.Description);
            logger.LogDebug("Parsed company {CompanyId} from XML", company.Id);
            return true;
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Cannot parse company XML: invalid document shape");
            return false;
        }
    }
}
