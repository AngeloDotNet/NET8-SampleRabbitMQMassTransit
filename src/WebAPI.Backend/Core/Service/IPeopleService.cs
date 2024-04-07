namespace WebAPI.Backend.Core.Service;

/// <summary>
/// Interface for the People service.
/// </summary>
public interface IPeopleService
{
    /// <summary>
    /// Asynchronously gets a list of all PersonEntity items.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of PersonEntity items.</returns>
    Task<List<PersonEntity>> GetListItemAsync();

    /// <summary>
    /// Asynchronously gets a PersonEntity item by its ID.
    /// </summary>
    /// <param name="id">The ID of the PersonEntity item.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the PersonEntity item.</returns>
    Task<PersonEntity> GetItemAsync(int id);

    /// <summary>
    /// Asynchronously creates a new PersonEntity item.
    /// </summary>
    /// <param name="item">The PersonEntity item to create.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task CreateItemAsync(PersonEntity item);

    /// <summary>
    /// Asynchronously updates a PersonEntity item.
    /// </summary>
    /// <param name="item">The PersonEntity item to update.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task UpdateItemAsync(PersonEntity item);

    /// <summary>
    /// Asynchronously deletes a PersonEntity item.
    /// </summary>
    /// <param name="item">The PersonEntity item to delete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task DeleteItemAsync(PersonEntity item);

    //Task DeleteItemByIdAsync(int id);

    //Task<List<PersonEntity>> GetPaginatedAsync(Func<IQueryable<PersonEntity>, IIncludableQueryable<PersonEntity, object>> includes,
    //    Expression<Func<PersonEntity, bool>> conditionWhere, Expression<Func<PersonEntity, dynamic>> orderBy, string orderType,
    //    int pageIndex, int pageSize);
}
