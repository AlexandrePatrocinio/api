using AutoCRUD.Data;
using AutoCRUD.Data.NpgSql;
using AutoCRUD.Models;
using AutoCRUD.Services;
using api.Models;

namespace api.Services;

public class ServiceCompanieValidation<E> : IServiceAutoCRUDValidation<E> where E : IEntity
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
        var vaidation = await IsValidEntityAsync(Entity, repository);
        if(!vaidation.Valid) return vaidation;
        
        var companie = (Companie) (vaidation.Entity ?? Entity);
        var companierepository = (NpgSqlRepository<Companie>)repository;

        if (string.IsNullOrWhiteSpace(companie.TradeName) || string.IsNullOrWhiteSpace(companie.BusinessSector)) return (false, companie) ;

        var found = await companierepository.FindByFieldAsync("TradeName", companie);

        return (found is null, companie);
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
        
        var companie = (Companie)(validation.Entity ?? Entity);
        var companierepository = (NpgSqlRepository<Companie>)repository;

        if (string.IsNullOrWhiteSpace(companie.TradeName) || string.IsNullOrWhiteSpace(companie.BusinessSector)) return (false, companie);

        var found = await companierepository.FindByFieldAsync("TradeName", companie);

        return (found is not null, found ?? companie);
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
