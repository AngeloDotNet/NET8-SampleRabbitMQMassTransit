using ClassLibrary.EFCore.Interfaces;

namespace WebAPI.Backend.Core.Service;

public class PeopleService : IPeopleService
{
    private readonly IRepository<PersonEntity, int> repository;

    public PeopleService(IRepository<PersonEntity, int> repository)
    {
        this.repository = repository;
    }

    public async Task<List<PersonEntity>> GetListItemAsync()
        => await repository.GetAllAsync();

    public async Task<PersonEntity> GetItemAsync(int id)
        => await repository.GetByIdAsync(id);

    public async Task CreateItemAsync(PersonEntity person)
        => await repository.CreateAsync(person);

    public async Task UpdateItemAsync(PersonEntity person)
        => await repository.UpdateAsync(person);

    public async Task DeleteItemAsync(PersonEntity person)
        => await repository.DeleteAsync(person);

    //public async Task DeleteItemByIdAsync(int id)
    //    => await repository.DeleteByIdAsync(id);

    //public Task<List<PersonEntity>> GetPaginatedAsync(Func<IQueryable<PersonEntity>, IIncludableQueryable<PersonEntity, object>> includes,
    //    Expression<Func<PersonEntity, bool>> conditionWhere, Expression<Func<PersonEntity, dynamic>> orderBy, string orderType,
    //    int pageIndex, int pageSize)

    //    => repository.GetPaginatedAsync(includes, conditionWhere, orderBy, orderType, pageIndex, pageSize);
}