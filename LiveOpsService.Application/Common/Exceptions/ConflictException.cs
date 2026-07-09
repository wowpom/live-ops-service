using LiveOpsService.Application.Common.Interfaces;

namespace LiveOpsService.Application.Common.Exceptions;

public class ConflictException(string message) : Exception(message), IAppException
{
    public int StatusCode => 409;
}