using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VectraPKM.Auth.Infrastructure;
using VectraPKM.Auth.Infrastructure.Identity;

namespace VectraPKM.Auth.Tests;

internal static class PasswordTestServices
{
    internal static ServiceProvider Create()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            // Only used to configure DI. The EF store is replaced, so tests never connect to PostgreSQL.
            ["ConnectionStrings:Auth"] = "Host=localhost;Database=unused_password_tests;Username=unused"
        }).Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthInfrastructure(configuration);
        services.AddSingleton<InMemoryUserStore>();
        services.Replace(ServiceDescriptor.Scoped<IUserStore<ApplicationUser>>(provider =>
            provider.GetRequiredService<InMemoryUserStore>()));

        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
    }
}
