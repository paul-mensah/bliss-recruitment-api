using BlissRecruitment.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace BlissRecruitment.Data.Data;

public class BlissRecruitmentDbContext : DbContext
{
    public BlissRecruitmentDbContext(DbContextOptions options) : base(options)
    {
        
    }
    
    public DbSet<QuestionEntity> Questions {get; set;}
}

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<BlissRecruitmentDbContext>
{
    public BlissRecruitmentDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false, true)
            .Build();

        string connectionStrings = configuration.GetConnectionString("DbConnection");
        
        var dbBuilder = new DbContextOptionsBuilder()
            .UseNpgsql(connectionStrings);

        return new BlissRecruitmentDbContext(dbBuilder.Options);
    }
}