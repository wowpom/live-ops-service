using System.Text.RegularExpressions;

namespace LiveOpsService.Models;

public static class RequestValidation
{
    public static (string Slug, string Name) ValidateNamedResource(string? slug, string? name)
    {
        var errors = new Dictionary<string, string[]>();
        if (slug is null || slug.Length is < 1 or > 64 || !Regex.IsMatch(slug, "\\A[a-z0-9-]+\\z"))
        {
            errors["slug"] = ["Slug must be 1–64 lowercase Latin letters, digits or hyphens."];
        }

        var trimmedName = name?.Trim();
        if (trimmedName is null || trimmedName.Length is < 1 or > 100)
        {
            errors["name"] = ["Name must be 1–100 characters after trimming whitespace."];
        }

        if (errors.Count > 0 || slug is null || trimmedName is null)
        {
            throw new RequestValidationException(errors);
        }

        return (slug, trimmedName);
    }

    public static string ValidateVersion(string? version)
    {
        if (version is null || !Regex.IsMatch(version, "\\A[0-9]+\\.[0-9]+\\.[0-9]+\\z"))
        {
            throw new RequestValidationException(new() { ["version"] = ["Version must use the X.Y.Z format, for example 1.0.0."] });
        }
        return version;
    }
}
