using api.Models;
using AutoCRUD.Extensions;
using Configuration.Models;

namespace Configuration.Extensions;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddEntityRepositories(this IServiceCollection services, DataBaseProviderType type, string connectionString)
    {
        switch (type)
        {
            case DataBaseProviderType.SqlServer:
                services.AddSqlClientRepository<Person,Guid>("Persons", "ID", connectionString, "Search");
                services.AddSqlClientRepository<Companie,Guid>("Companies", "ID", connectionString, "Search");
                break;
            case DataBaseProviderType.PostgreSql:
                services.AddNpgSqlRepository<Person,Guid>("Persons", "ID", connectionString, "Search");
                services.AddNpgSqlRepository<Companie,Guid>("Companies", "ID", connectionString, "Search");
                break;
            case DataBaseProviderType.MySql:
                throw new NotImplementedException("MySql support is not implemented yet.");
        }

        return services;
    }
}
