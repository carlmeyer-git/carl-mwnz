namespace Mwnz.Api.Configuration;

public sealed class XmlApiOptions
{
    public const string SectionName = "XmlApi";

    public string BaseUrl { get; set; } =
        "https://raw.githubusercontent.com/MiddlewareNewZealand/evaluation-instructions/main/xml-api";

    public int TimeoutSeconds { get; set; } = 30;
}
