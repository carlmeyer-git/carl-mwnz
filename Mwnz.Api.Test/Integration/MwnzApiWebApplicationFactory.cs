using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Mwnz.Api.Services;

namespace Mwnz.Api.Test.Integration;

public sealed class MwnzApiWebApplicationFactory : WebApplicationFactory<Program>
{
    public FakeXmlCompanyClient FakeXmlClient { get; } = new();

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

            services.AddSingleton<IXmlCompanyClient>(FakeXmlClient);
        });
    }
}

public sealed class FakeXmlCompanyClient : IXmlCompanyClient
{
    public Func<int, XmlFetchResult>? FetchHandler { get; set; }

    public Task<XmlFetchResult> FetchCompanyXmlAsync(int companyId, CancellationToken cancellationToken = default)
    {
        if (FetchHandler is null)
        {
            return Task.FromResult(new XmlFetchResult(XmlFetchStatus.UpstreamError));
        }

        return Task.FromResult(FetchHandler(companyId));
    }
}
