using AutoCRUD.Data;
using AutoCRUD.Models;
using AutoCRUD.Services;
using api.Models;

namespace api.Services;

public class ServicePersonValidation : IServiceAutoCRUDValidation<Person,Guid> 
{
    public async Task<(bool Valid, IEntity<Guid>? Entity)> IsValidEntityAsync(IEntity<Guid> Entity, IRepository<Person,Guid> repository)
    {
        return await Task.FromResult((Entity is not null, Entity));
    }

    public (bool, Guid) isValidID(string id, IRepository<Person,Guid> repository)
    {
            Guid ID;
            return (Guid.TryParse(id, out ID), ID);
    }

    public (bool Valid, string SearchTerm) isSearchTermValid(string t, IRepository<Person,Guid> repository)
    {
        return (true, t);
    }

    public async Task<(bool Valid, IEntity<Guid>? Entity)> isPostValidAsync(IEntity<Guid> Entity, IRepository<Person,Guid> repository)
    {
        var validation = await IsValidEntityAsync(Entity, repository);
        if(!validation.Valid) return validation;
        
        var person = (Person)(validation.Entity ?? Entity);

        if (string.IsNullOrWhiteSpace(person.Name) || string.IsNullOrWhiteSpace(person.Alias)) return (false, person);

        var found = await repository.FindByFieldAsync("Alias", person);

        return (found is null, person);
    }

    public async Task<(bool Valid, IEntity<Guid>? Entity)> isGetValidAsync(IEntity<Guid> Entity, IRepository<Person,Guid> repository)
    {
        return await IsValidEntityAsync(Entity, repository);
    }

    public async Task<(bool Valid, IEntity<Guid>? Entity)> isGetValidAsync(Guid id, IRepository<Person,Guid> repository)
    {
        return await Task.FromResult<(bool, IEntity<Guid>?)>((true, null));
    }

    public async Task<(bool Valid, IEntity<Guid>? Entity)> isPutValidAsync(IEntity<Guid> Entity, IRepository<Person,Guid> repository)
    {
        var validation = await IsValidEntityAsync(Entity, repository);
        if(!validation.Valid) return validation;
        
        var person = (Person)(validation.Entity ?? Entity);

        if (string.IsNullOrWhiteSpace(person.Name) || string.IsNullOrWhiteSpace(person.Alias)) return (false, person);

        var found = await repository.FindByFieldAsync("ID", person) ?? await repository.FindByFieldAsync("Alias", person);

        if (found is not null)
            person.Id = found.Id;

        return (found is not null, person);
    }

    public async Task<(bool Valid, IEntity<Guid>? Entity)> isDeleteValidAsync(IEntity<Guid> Entity, IRepository<Person,Guid> repository)
    {
        return await IsValidEntityAsync(Entity, repository);
    }

    public async Task<(bool Valid, IEntity<Guid>? Entity)> isDeleteValidAsync(Guid id, IRepository<Person,Guid> repository)
    {
        return await Task.FromResult<(bool, IEntity<Guid>?)>((true, null));
    }
}
