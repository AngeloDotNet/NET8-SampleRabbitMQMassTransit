namespace WebAPI.Backend.Controllers;

/// <summary>
/// Controller for managing people.
/// </summary>
public class PeopleController(IPeopleService peopleService) : BaseController
{
    private readonly IPeopleService peopleService = peopleService;

    /// <summary>
    /// Gets a list of all people.
    /// </summary>
    /// <returns>A list of all people.</returns>
    [HttpGet]
    public async Task<IActionResult> GetPeopleAsync()
    {
        var people = await peopleService.GetListItemAsync();
        return Ok(people);
    }

    /// <summary>
    /// Gets a specific person by their ID.
    /// </summary>
    /// <param name="id">The ID of the person.</param>
    /// <returns>The person with the given ID, or NotFound if no such person exists.</returns>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetPersonAsync(int id)
    {
        var person = await peopleService.GetItemAsync(id);

        if (person == null)
        {
            return NotFound(person);
        }

        return Ok(person);
    }

    /// <summary>
    /// Creates a new person.
    /// </summary>
    /// <param name="entity">The person to create.</param>
    /// <returns>Ok if the person was created successfully.</returns>
    [HttpPost]
    public async Task<IActionResult> CreatePersonAsync([FromBody] PersonEntity entity)
    {
        await peopleService.CreateItemAsync(entity);
        return Ok();
    }

    /// <summary>
    /// Edits an existing person.
    /// </summary>
    /// <param name="entity">The person to edit.</param>
    /// <returns>Ok if the person was edited successfully.</returns>
    [HttpPut]
    public async Task<IActionResult> EditPersonAsync([FromBody] PersonEntity entity)
    {
        await peopleService.UpdateItemAsync(entity);
        return Ok();
    }

    /// <summary>
    /// Deletes a specific person by their ID.
    /// </summary>
    /// <param name="id">The ID of the person to delete.</param>
    /// <returns>Ok if the person was deleted successfully, or NotFound if no such person exists.</returns>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeletePersonAsync(int id)
    {
        var person = await peopleService.GetItemAsync(id);

        if (person == null)
        {
            return NotFound();
        }

        await peopleService.DeleteItemAsync(person);
        return Ok();
    }
}
