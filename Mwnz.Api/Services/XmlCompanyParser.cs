using System.Xml.Serialization;
using Mwnz.Api.Models;

namespace Mwnz.Api.Services;

public sealed class XmlCompanyParser : IXmlCompanyParser
{
    private static readonly XmlSerializer Serializer = new(typeof(XmlCompanyData));

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
            if (Serializer.Deserialize(reader) is not XmlCompanyData data)
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
