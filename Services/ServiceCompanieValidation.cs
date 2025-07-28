using AutoCRUD.Data;
using AutoCRUD.Models;
using AutoCRUD.Services;
using api.Models;

namespace api.Services;

public class ServiceCompanieValidation : IServiceAutoCRUDValidation<Companie,Guid> 
{
    public async Task<(bool Valid, IEntity<Guid>? Entity)> IsValidEntityAsync(IEntity<Guid> Entity, IRepository<Companie,Guid> repository)
    {
        return await Task.FromResult((Entity is not null, Entity));
    }

    public (bool, Guid) isValidID(string id, IRepository<Companie,Guid> repository)
    {
            Guid ID;
            return (Guid.TryParse(id, out ID), ID);
    }

    public (bool Valid, string SearchTerm) isSearchTermValid(string t, IRepository<Companie,Guid> repository)
    {
        return (true, t);
    }

    public async Task<(bool Valid, IEntity<Guid>? Entity)> isPostValidAsync(IEntity<Guid> Entity, IRepository<Companie,Guid> repository)
    {
        var vaidation = await IsValidEntityAsync(Entity, repository);
        if(!vaidation.Valid) return vaidation;
        
        var companie = (Companie) (vaidation.Entity ?? Entity);

        if (string.IsNullOrWhiteSpace(companie.TradeName) || string.IsNullOrWhiteSpace(companie.BusinessSector)) return (false, companie) ;

        var found = await repository.FindByFieldAsync("TradeName", companie);

        return (found is null, companie);
    }

    public async Task<(bool Valid, IEntity<Guid>? Entity)> isGetValidAsync(IEntity<Guid> Entity, IRepository<Companie,Guid> repository)
    {
        return await IsValidEntityAsync(Entity, repository);
    }

    public async Task<(bool Valid, IEntity<Guid>? Entity)> isGetValidAsync(Guid id, IRepository<Companie,Guid> repository)
    {
        return await Task.FromResult<(bool, IEntity<Guid>?)>((true, null));
    }

    public async Task<(bool Valid, IEntity<Guid>? Entity)> isPutValidAsync(IEntity<Guid> Entity, IRepository<Companie,Guid> repository)
    {
        var validation = await IsValidEntityAsync(Entity, repository);
        if(!validation.Valid) return validation;
        
        var companie = (Companie)(validation.Entity ?? Entity);

        if (string.IsNullOrWhiteSpace(companie.TradeName) || string.IsNullOrWhiteSpace(companie.BusinessSector)) return (false, companie);

        var found = await repository.FindByFieldAsync("TradeName", companie);

        return (found is not null, companie);
    }

    public async Task<(bool Valid, IEntity<Guid>? Entity)> isDeleteValidAsync(IEntity<Guid> Entity, IRepository<Companie,Guid> repository)
    {
        return await IsValidEntityAsync(Entity, repository);
    }

    public async Task<(bool Valid, IEntity<Guid>? Entity)> isDeleteValidAsync(Guid id, IRepository<Companie,Guid> repository)
    {
        return await Task.FromResult<(bool, IEntity<Guid>?)>((true, null));
    }
}
