using Configuration.Models;

namespace Configuration.Extensions;

public static class IConfigurationRootExtensions
{
    public static (DataBaseProviderType type, string connectionString) GetDatabaseConfigurations(this IConfigurationRoot configuration)
    {
        var databaseProviderSection = configuration.GetSection("database");
        string connectionStringKey = databaseProviderSection.GetValue<string>("ConnectionStringKey")
            ?? throw new ArgumentNullException("ConnectionStringKey not found in configuration.");

        string connectionString = configuration.GetConnectionString(connectionStringKey)
            ?? throw new ArgumentNullException("ConnectionString not found in configuration.");

        DataBaseProviderType type = databaseProviderSection.GetValue("Provider", DataBaseProviderType.PostgreSql);

        return (type, connectionString);
    }
}