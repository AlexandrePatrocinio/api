using api.Models;
using api.Services;
using AutoCRUD.Extensions;

var env = Environment.GetEnvironmentVariables();

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration.SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "Configuration"))
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddJsonFile($"appsettings.{env["EnvironmentName"]}.json", optional: true, reloadOnChange: true)
        .AddUserSecrets<Program>()
        .Build();

var app = builder
    .AddServiceAutoCRUDValidation<Person, ServicePersonValidation<Person>>()
    .AddSqlClientRepository<Person>("Persons", "ID", configuration.GetConnectionString("api") ?? string.Empty, "Search")
    .AddSqlClientRepository<Companie>("Companies", "ID", configuration.GetConnectionString("api") ?? string.Empty, "Search")    
    .Build();

app
    .UseAutoCRUD<Person>()
    .UseAutoCRUD<Companie>()
    .Run();