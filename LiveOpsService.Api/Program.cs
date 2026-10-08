using LiveOpsService.Application.Common.Interfaces;
using LiveOpsService.Application.Services;
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

        builder.Services.AddSingleton<InMemoryStore>();
        builder.Services.AddSingleton<IProjectRepository, InMemoryProjectRepository>();
        builder.Services.AddSingleton<IConfigRepository, InMemoryConfigRepository>();
        builder.Services.AddScoped<ConfigService>();
        builder.Services.AddProblemDetails();
        builder.Services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = true);
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        
        var app = builder.Build();
        
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseExceptionHandler();
        app.UseStatusCodePages(async context =>
        {
            await Results.Problem(
                statusCode: context.HttpContext.Response.StatusCode,
                extensions: new Dictionary<string, object?> { ["traceId"] = context.HttpContext.TraceIdentifier })
                .ExecuteAsync(context.HttpContext);
        });

        app.MapAdminEndpoints();
        app.MapConfigEndpoints();

        app.MapGet("/", () => Results.Redirect("/swagger"));

        app.Run();
        
    }
}
