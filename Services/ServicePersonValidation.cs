using AutoCRUD.Data;
using AutoCRUD.Data.SqlClient;
using AutoCRUD.Models;
using AutoCRUD.Services;
using api.Models;

namespace api.Services;

public class ServicePersonValidation<E> : IServiceAutoCRUDValidation<E> where E : IEntity
{
    public async Task<(bool Valid, IEntity? Entity)> IsValidEntityAsync(IEntity Entity, IRepository<E> repository)
    {
        return await Task<(bool, IEntity)>.FromResult((Entity is not null, Entity));
    }

    public (bool, Guid) isValidID(string id, IRepository<E> repository)
    {
            Guid ID;
            return (Guid.TryParse(id, out ID), ID);
    }

    public (bool Valid, string SearchTerm) isSearchTermValid(string t, IRepository<E> repository)
    {
        return (string.IsNullOrEmpty(t), t);
    }

    public async Task<(bool Valid, IEntity? Entity)> isPostValidAsync(IEntity Entity, IRepository<E> repository)
    {
        var validation = await IsValidEntityAsync(Entity, repository);
        if(!validation.Valid) return validation;
        
        var person = (Person)(validation.Entity ?? Entity);
        var personrepository = (SqlClientRepository<Person>)repository;

        if (string.IsNullOrWhiteSpace(person.Name) || string.IsNullOrWhiteSpace(person.Alias)) return (false, person);

        var found = await personrepository.FindByFieldAsync("Alias", person);

        return (found is null, person);
    }

    public async Task<(bool Valid, IEntity? Entity)> isGetValidAsync(IEntity Entity, IRepository<E> repository)
    {
        return await IsValidEntityAsync(Entity, repository);
    }

    public async Task<(bool Valid, IEntity? Entity)> isGetValidAsync(Guid id, IRepository<E> repository)
    {
        return await Task.FromResult<(bool, IEntity?)>((true, null));
    }

    public async Task<(bool Valid, IEntity? Entity)> isPutValidAsync(IEntity Entity, IRepository<E> repository)
    {
        var validation = await IsValidEntityAsync(Entity, repository);
        if(!validation.Valid) return validation;
        
        var person = (Person)(validation.Entity ?? Entity);
        var personrepository = (SqlClientRepository<Person>)repository;

        if (string.IsNullOrWhiteSpace(person.Name) || string.IsNullOrWhiteSpace(person.Alias)) return (false, person);

        var found = await personrepository.FindByFieldAsync("ID", person) ?? await personrepository.FindByFieldAsync("Alias", person);

        if (found is not null)
            person.Id = found.Id;

        return (found is not null, person);
    }

    public async Task<(bool Valid, IEntity? Entity)> isDeleteValidAsync(IEntity Entity, IRepository<E> repository)
    {
        return await IsValidEntityAsync(Entity, repository);
    }

    public async Task<(bool Valid, IEntity? Entity)> isDeleteValidAsync(Guid id, IRepository<E> repository)
    {
        return await Task.FromResult<(bool, IEntity?)>((true, null));
    }
}
