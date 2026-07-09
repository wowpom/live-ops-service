using LiveOpsService.Application.Common.Interfaces;

namespace LiveOpsService.Application.Common.Exceptions;

public class NotFoundException(string message) : Exception(message), IAppException
{
    public int StatusCode => 404;
}