using System.Xml.Serialization;
using Mwnz.Api.Models;
using ExternalXmlCompany = Mwnz.Api.Integrations.XmlCompany.Models.XmlCompany;

namespace Mwnz.Api.Integrations.XmlCompany;

public sealed class XmlCompanyParser : IXmlCompanyParser
{
    private static readonly XmlSerializer Serializer = new(typeof(ExternalXmlCompany));

    public bool TryParse(string xml, out Company? company)
    {
        company = null;

        if (string.IsNullOrWhiteSpace(xml))
        {
            return false;
        }

        try
        {
            using var reader = new StringReader(xml);
            if (Serializer.Deserialize(reader) is not ExternalXmlCompany data)
            {
                return false;
            }

            company = new Company(data.Id, data.Name, data.Description);
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }
}
