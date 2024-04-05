namespace WebAPI.Frontend.Controllers;

public class HomeController : BaseController
{
    private readonly IRequestClient<PeopleListRequest> peopleRequest;
    private readonly IRequestClient<PersonRequest> personRequest;

    public HomeController(IRequestClient<PeopleListRequest> peopleRequest, IRequestClient<PersonRequest> personRequest)
    {
        this.peopleRequest = peopleRequest;
        this.personRequest = personRequest;
    }

    /// <summary>
    /// Get all people
    /// </summary>
    /// <returns>Return list of people</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/Home
    ///
    /// </remarks>
    /// <response code="200">Get list of people</response>
    /// <response code="204">No content</response>
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
    /// Get person by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns>Return person by id</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/Home/{id}
    ///
    /// </remarks>
    /// <response code="200">Get person by id</response>
    /// <response code="404">Person not found</response>
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