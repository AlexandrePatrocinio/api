namespace Configuration.Models;

public enum DataBaseProviderType
{
    SqlServer,
    MySql,
    PostgreSql
}

public class DataBaseProvider
{
    public DataBaseProviderType Provider = DataBaseProviderType.PostgreSql;

    public string ConnectionStringKey = string.Empty;
}