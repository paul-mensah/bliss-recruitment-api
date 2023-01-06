using System.Reflection;
using Arch.EntityFrameworkCore.UnitOfWork;
using BlissRecruitment.Core.Options;
using BlissRecruitment.Core.Repositories;
using BlissRecruitment.Core.Services.Email;
using BlissRecruitment.Core.Services.Questions;
using BlissRecruitment.Data.Data;
using BlissRecruitment.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

namespace BlissRecruitment.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Bliss Recruitment Api",
                Version = "v1",
                Description = "Bliss Recruitment Api",
                Contact = new OpenApiContact
                {
                    Name = "Paul Mensah",
                    Email = "paulmensah1409@gmail.com"
                }
            });
            c.ResolveConflictingActions(resolver => resolver.First());
            c.EnableAnnotations();
            
            string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);
        });
    }

    public static void AddCustomServicesAndConfigurations(this IServiceCollection services, IConfiguration configuration)
    {
        // Configuration
        services.Configure<EmailConfiguration>(configuration.GetSection(nameof(EmailConfiguration)));
        
        // Services
        services.AddScoped<IQuestionsRepository, QuestionsRepository>();
        services.AddScoped<IQuestionsService, QuestionsService>();
        services.AddScoped<IEmailService, EmailService>();
    }

    public static void AddPostgresSqlDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<BlissRecruitmentDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DbConnection"));
        }, ServiceLifetime.Transient).AddUnitOfWork<BlissRecruitmentDbContext>();
    }
}