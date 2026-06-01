namespace Mwnz.Api.Integrations.XmlCompany;

public interface IXmlCompanyClient
{
    Task<XmlFetchResult> FetchCompanyXmlAsync(int companyId, CancellationToken cancellationToken = default);
}
