namespace WebAPI.Backend.Controllers;

public class PeopleController : BaseController
{
    private readonly IPeopleService peopleService;

    public PeopleController(IPeopleService peopleService)
    {
        this.peopleService = peopleService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPeopleAsync()
    {
        var people = await peopleService.GetListItemAsync();
        return Ok(people);
    }

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

    [HttpPost]
    public async Task<IActionResult> CreatePersonAsync([FromBody] PersonEntity entity)
    {
        await peopleService.CreateItemAsync(entity);
        return Ok();
    }

    [HttpPut]
    public async Task<IActionResult> EditPersonAsync([FromBody] PersonEntity entity)
    {
        await peopleService.UpdateItemAsync(entity);
        return Ok();
    }

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