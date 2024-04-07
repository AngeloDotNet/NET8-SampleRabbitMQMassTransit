namespace WebAPI.Frontend.Controllers;

/// <summary>
/// Controller for handling requests related to people.
/// </summary>
public class HomeController(IRequestClient<PeopleListRequest> peopleRequest, IRequestClient<PersonRequest> personRequest) : BaseController
{
    private readonly IRequestClient<PeopleListRequest> peopleRequest = peopleRequest;
    private readonly IRequestClient<PersonRequest> personRequest = personRequest;

    /// <summary>
    /// Gets a list of all people.
    /// </summary>
    /// <returns>A list of people if found, otherwise NoContent.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<PersonEntity>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetPeopleAsync()
    {
        var people = new List<PersonEntity>();

        using (var request = peopleRequest.Create(new PeopleListRequest { }))
        {
            var response = await request.GetResponse<PeopleListResponse>();

            if (response.Message.People.Count == 0)
            {
                return NoContent();
            }

            people = response.Message.People;
        }

        return Ok(people);
    }

    /// <summary>
    /// Gets a person by their ID.
    /// </summary>
    /// <param name="id">The ID of the person.</param>
    /// <returns>The person if found, otherwise NotFound.</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PersonEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPersonAsync(int id)
    {
        var person = new PersonEntity();

        using (var request = personRequest.Create(new PersonRequest { Id = id }))
        {
            var response = await request.GetResponse<PersonResponse>();

            if (response.Message.Person == null)
            {
                return NotFound();
            }

            person = response.Message.Person;
        }

        return Ok(person);
    }
}
