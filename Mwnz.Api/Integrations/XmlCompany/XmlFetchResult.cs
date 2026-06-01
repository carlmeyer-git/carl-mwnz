namespace Mwnz.Api.Integrations.XmlCompany;

public enum XmlFetchStatus
{
    Success,
    NotFound,
    UpstreamError
}

public sealed record XmlFetchResult(XmlFetchStatus Status, string? XmlContent = null);
