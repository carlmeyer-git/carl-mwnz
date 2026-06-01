using System.Xml.Serialization;

namespace Mwnz.Api.Integrations.XmlCompany.Models;

[XmlRoot("Data")]
public sealed class XmlCompany
{
    [XmlElement("id")]
    public int Id { get; set; }

    [XmlElement("name")]
    public string Name { get; set; } = string.Empty;

    [XmlElement("description")]
    public string Description { get; set; } = string.Empty;
}
