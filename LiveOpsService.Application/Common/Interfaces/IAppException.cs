namespace LiveOpsService.Application.Common.Interfaces;

public interface IAppException
{
    int StatusCode { get; }
    string Message { get; }
}