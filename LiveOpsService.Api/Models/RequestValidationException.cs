namespace LiveOpsService.Models;

public class RequestValidationException(Dictionary<string, string[]> errors) : Exception("Request validation failed.")
{
    public Dictionary<string, string[]> Errors { get; } = errors;
}
