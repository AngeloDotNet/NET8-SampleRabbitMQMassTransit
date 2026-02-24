using SampleMicroservice.Messaging;

namespace WebAPI.Frontend.Controllers;

public class HomeController(IRabbitMqBus bus) : BaseController
{
    [HttpGet]
    [ProducesResponseType(typeof(List<PersonEntity>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPeopleAsync()
    {
        var response = await bus.RequestAsync<PeopleListRequest, PeopleListResponse>(Settings.QueueNameRequest, new PeopleListRequest(), TimeSpan.FromSeconds(10));

        if (response?.People == null || response.People.Count == 0)
        {
            return NotFound();
        }

        return Ok(response.People);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PersonEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPersonAsync(int id)
    {
        var response = await bus.RequestAsync<PersonRequest, PersonResponse>(Settings.QueueNamePerson, new PersonRequest { Id = id }, TimeSpan.FromSeconds(10));

        if (response?.Person == null)
        {
            return NotFound();
        }

        return Ok(response.Person);
    }
}