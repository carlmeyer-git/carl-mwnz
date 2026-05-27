using Mwnz.Api.Configuration;
using Mwnz.Api.Endpoints;
using Mwnz.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<XmlApiOptions>(builder.Configuration.GetSection(XmlApiOptions.SectionName));
builder.Services.AddOpenApi();

builder.Services.AddSingleton<IXmlCompanyParser, XmlCompanyParser>();
builder.Services.AddScoped<ICompanyService, CompanyService>();

builder.Services.AddHttpClient<IXmlCompanyClient, XmlCompanyClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<XmlApiOptions>>().Value;
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
    .ExcludeFromDescription();

app.MapCompanyEndpoints();

app.Run();

public partial class Program;
