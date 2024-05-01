using api.Models;
using api.Services;
using AutoCRUD.Extensions;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

var app = builder
    .AddServiceAutoCRUDValidation<Person, ServicePersonValidation<Person>>()
    .AddNpgSqlRepository<Person>("Persons", "ID", configuration.GetConnectionString("api") ?? string.Empty, "Search")
    .AddServiceAutoCRUDValidation<Companie, ServiceCompanieValidation<Companie>>()
    .AddNpgSqlRepository<Companie>("Companies", "ID", configuration.GetConnectionString("api") ?? string.Empty, "Search")    
    .Build();

app
    .UseAutoCRUD<Person>()
    .UseAutoCRUD<Companie>()
    .Run();