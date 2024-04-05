namespace WebAPI.Backend.Core.Consumers;

public class ConsumerPersonRequest : IConsumer<PersonRequest>
{
    private readonly IPeopleService peopleService;

    public ConsumerPersonRequest(IPeopleService peopleService)
    {
        this.peopleService = peopleService;
    }

    public async Task Consume(ConsumeContext<PersonRequest> context)
    {
        var personId = context.Message.Id;
        var person = await peopleService.GetItemAsync(personId);

        await context.RespondAsync(new PersonResponse { Person = person });
    }
}