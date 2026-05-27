namespace Mwnz.Api.Services;

public interface IXmlCompanyClient
{
    Task<XmlFetchResult> FetchCompanyXmlAsync(int companyId, CancellationToken cancellationToken = default);
}

public enum XmlFetchStatus
{
    Success,
    NotFound,
    UpstreamError
}

public sealed record XmlFetchResult(XmlFetchStatus Status, string? XmlContent = null);
