namespace WebAPI.Backend.Core.Service;

public interface IPeopleService
{
    Task<List<PersonEntity>> GetListItemAsync();
    Task<PersonEntity> GetItemAsync(int id);
    Task CreateItemAsync(PersonEntity item);
    Task UpdateItemAsync(PersonEntity item);
    Task DeleteItemAsync(PersonEntity item);

    //Task DeleteItemByIdAsync(int id);

    //Task<List<PersonEntity>> GetPaginatedAsync(Func<IQueryable<PersonEntity>, IIncludableQueryable<PersonEntity, object>> includes,
    //    Expression<Func<PersonEntity, bool>> conditionWhere, Expression<Func<PersonEntity, dynamic>> orderBy, string orderType,
    //    int pageIndex, int pageSize);
}