using System.Text.Json.Serialization;

namespace Mwnz.Api.Models;

public sealed record ApiError(
    [property: JsonPropertyName("error")] string Error,
    [property: JsonPropertyName("error_description")] string ErrorDescription);
