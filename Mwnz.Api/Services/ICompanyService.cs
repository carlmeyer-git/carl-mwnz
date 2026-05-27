namespace Mwnz.Api.Services;

public interface ICompanyService
{
    Task<CompanyResult> GetCompanyAsync(int companyId, CancellationToken cancellationToken = default);
}
