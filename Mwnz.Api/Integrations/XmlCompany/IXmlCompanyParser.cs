using Mwnz.Api.Models;

namespace Mwnz.Api.Integrations.XmlCompany;

public interface IXmlCompanyParser
{
    bool TryParse(string xml, out Company? company);
}
