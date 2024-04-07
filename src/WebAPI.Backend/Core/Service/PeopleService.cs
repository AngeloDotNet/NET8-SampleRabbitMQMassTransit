using ClassLibrary.EFCore.Interfaces;

namespace WebAPI.Backend.Core.Service;

/// <summary>
/// Service class for managing people.
/// </summary>
/// <param name="repository">The repository to use for data access.</param>
public class PeopleService(IRepository<PersonEntity, int> repository) : IPeopleService
{
    private readonly IRepository<PersonEntity, int> repository = repository;

    /// <summary>
    /// Gets a list of all people.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of people.</returns>
    public async Task<List<PersonEntity>> GetListItemAsync() => await repository.GetAllAsync();

    /// <summary>
    /// Gets a person by their ID.
    /// </summary>
    /// <param name="id">The ID of the person to get.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the person entity.</returns>
    public async Task<PersonEntity> GetItemAsync(int id) => await repository.GetByIdAsync(id);

    /// <summary>
    /// Creates a new person.
    /// </summary>
    /// <param name="person">The person to create.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task CreateItemAsync(PersonEntity person) => await repository.CreateAsync(person);

    /// <summary>
    /// Updates an existing person.
    /// </summary>
    /// <param name="person">The person to update.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task UpdateItemAsync(PersonEntity person) => await repository.UpdateAsync(person);

    /// <summary>
    /// Deletes a person.
    /// </summary>
    /// <param name="person">The person to delete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task DeleteItemAsync(PersonEntity person) => await repository.DeleteAsync(person);

    //public async Task DeleteItemByIdAsync(int id)
    //    => await repository.DeleteByIdAsync(id);

    //public Task<List<PersonEntity>> GetPaginatedAsync(Func<IQueryable<PersonEntity>, IIncludableQueryable<PersonEntity, object>> includes,
    //    Expression<Func<PersonEntity, bool>> conditionWhere, Expression<Func<PersonEntity, dynamic>> orderBy, string orderType,
    //    int pageIndex, int pageSize)

    //    => repository.GetPaginatedAsync(includes, conditionWhere, orderBy, orderType, pageIndex, pageSize);
}
