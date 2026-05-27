using Mwnz.Api.Models;

namespace Mwnz.Api.Services;

public enum CompanyResultKind
{
    Success,
    NotFound,
    UpstreamError,
    InvalidResponse
}

public sealed record CompanyResult(
    CompanyResultKind Kind,
    Company? Company = null,
    ApiError? Error = null);
