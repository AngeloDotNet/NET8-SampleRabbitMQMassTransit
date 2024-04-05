namespace WebAPI.Backend.Core.Consumers;

public class ConsumerPersonListRequest : IConsumer<PeopleListRequest>
{
    private readonly IPeopleService peopleService;

    public ConsumerPersonListRequest(IPeopleService peopleService)
    {
        this.peopleService = peopleService;
    }

    public async Task Consume(ConsumeContext<PeopleListRequest> context)
    {
        var listPeople = await peopleService.GetListItemAsync();

        await context.RespondAsync(new PeopleListResponse { People = listPeople });
    }
}