using BlissRecruitment.Api.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BlissRecruitment.Tests.TestSetup;

public class TestFixture
{
    public ServiceProvider ServiceProvider { get; }
    
    public TestFixture()
    {
        var services = new ServiceCollection();
        ConfigurationManager.SetupConfiguration();

        services.AddSingleton(sp => ConfigurationManager.Configuration);

        services.AddLogging(x => x.AddConsole());
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        services.AddCustomServicesAndConfigurations(ConfigurationManager.Configuration);
        
        ServiceProvider = services.BuildServiceProvider();
    }
}