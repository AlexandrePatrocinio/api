using api.Models;
using api.Services;
using AutoCRUD.Extensions;
using Configuration.Extensions;
using Configuration.Models;

var env = Environment.GetEnvironmentVariables();

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration.SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "Configuration"))
        .AddJsonFile("appsettings.json")
        .AddJsonFile($"appsettings.{env["ASPNETCORE_ENVIRONMENT"]}.json", optional: true, reloadOnChange: true)
        .AddUserSecrets<Program>()
        .Build();

(DataBaseProviderType type, string connectionString) = configuration.GetDatabaseConfigurations();

builder
    .Services
        .AddEntityRepositories(type, connectionString)
        .Configure<DataBaseProvider>(configuration.GetSection("database"))
        .AddServiceAutoCRUDValidation<Person, Guid, ServicePersonValidation>()
        .AddServiceAutoCRUDValidation<Companie, Guid, ServiceCompanieValidation>()
        .AddCors((options) => options.AddDefaultPolicy((policy) =>
            {
                policy.AllowAnyHeader();
                policy.AllowAnyOrigin();
                policy.AllowAnyMethod();
            })
        );

var app = builder.Build();

app
    .UseAutoCRUD<Person, Guid>()
    .UseAutoCRUD<Companie, Guid>();

app.UseCors();

app.Run();