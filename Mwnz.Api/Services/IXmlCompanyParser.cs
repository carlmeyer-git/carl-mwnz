using Mwnz.Api.Models;

namespace Mwnz.Api.Services;

public interface IXmlCompanyParser
{
    bool TryParse(string xml, out Company? company);
}
