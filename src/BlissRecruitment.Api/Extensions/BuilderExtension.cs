using System.Text;
using System.Text.Json;
using BlissRecruitment.Api.Middlewares;
using BlissRecruitment.Data.Data;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BlissRecruitment.Api.Extensions;

public static class BuilderExtension
{
    public static WebApplication BuildApplication(this WebApplicationBuilder builder)
    {
        builder.Services.AddSwaggerDocumentation();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        builder.Services.AddCors();
        builder.Services.AddControllers().AddJsonOptions(o =>
        {
            o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        });
        builder.Services.Configure<RouteOptions>(o => o.LowercaseUrls = true);
        builder.Services.AddPostgresSqlDatabase(builder.Configuration);
        builder.Services.AddCustomServicesAndConfigurations(builder.Configuration);
        builder.Services.AddHealthChecks();
        
        return builder.Build();
    }

    public static void RunApplication(this WebApplication application)
    {
        // Check if there are pending migrations and execute
        RunMigrations(application.Services).GetAwaiter().GetResult();
        
        // Configure the HTTP request pipeline.
        if (application.Environment.IsDevelopment())
        {
            application.UseSwagger();
            application.UseSwaggerUI(s => 
            {
                s.SwaggerEndpoint("/swagger/v1/swagger.json", "Bliss Recruitment Api");
            });
        }

        application.UseCors(x => x
            .AllowAnyMethod()
            .AllowAnyHeader()
            .SetIsOriginAllowed(origin => true)
            .AllowCredentials());

        application.UseRouting();
        application.ConfigureGlobalHandler(application.Logger);
        application.UseHttpsRedirection();
        application.UseAuthorization();
        application.MapControllers();
        application.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = WriteResponse
        });

        application.Run();
    }

    private static async Task RunMigrations(IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<BlissRecruitmentDbContext>();
        
            int pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync()).Count();
            
            if (pendingMigrations >= 1)
            {
                await dbContext.Database.MigrateAsync();
                logger.LogDebug("{pendingMigrations} migrations successfully executed ", pendingMigrations);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occured executing pending migrations");
        }
    }
    
    private static Task WriteResponse(HttpContext context, HealthReport healthReport)
    {
        context.Response.ContentType = "application/json; charset=utf-8";

        var options = new JsonWriterOptions { Indented = true };

        using var memoryStream = new MemoryStream();
        using (var jsonWriter = new Utf8JsonWriter(memoryStream, options))
        {
            jsonWriter.WriteStartObject();

            string status =  healthReport.Status == HealthStatus.Degraded
                ? "Service Unavailable. Please try again later."
                : "OK";
            
            jsonWriter.WriteString("status", status);

            context.Response.StatusCode = healthReport.Status == HealthStatus.Degraded
                ? StatusCodes.Status503ServiceUnavailable
                : StatusCodes.Status200OK;

            jsonWriter.WriteEndObject();
        }

        return context.Response.WriteAsync(
            Encoding.UTF8.GetString(memoryStream.ToArray()));
    }
}