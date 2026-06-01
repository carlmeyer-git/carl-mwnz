using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Mwnz.Api.Integrations.XmlCompany;

namespace Mwnz.Api.Test.Integration;

public sealed class MwnzApiWebApplicationFactory : WebApplicationFactory<Program>
{
    public Mock<IXmlCompanyClient> XmlCompanyClientMock { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptors = services
                .Where(d => d.ServiceType == typeof(IXmlCompanyClient))
                .ToList();

            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            services.AddSingleton(XmlCompanyClientMock.Object);
        });
    }
}
