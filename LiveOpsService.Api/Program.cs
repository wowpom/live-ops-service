using LiveOpsService.Application.Common.Interfaces;
using LiveOpsService.Endpoints;
using LiveOpsService.Infrastructure.Persistence;
using LiveOpsService.Middleware;

namespace LiveOpsService;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddSingleton<IProjectRepository, InMemoryProjectRepository>();
        builder.Services.AddProblemDetails();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        
        var app = builder.Build();
        
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseExceptionHandler();

        app.MapAdminEndpoints();

        app.MapGet("/", () => Results.Redirect("/swagger"));

        app.Run();
        
    }
}