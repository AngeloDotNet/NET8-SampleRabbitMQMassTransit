namespace WebAPI.Backend.Core.Consumers;

/// <summary>
/// Consumer for handling PeopleListRequest messages.
/// </summary>
/// <param name="peopleService">Service for handling people related operations.</param>
public class ConsumerPersonListRequest(IPeopleService peopleService) : IConsumer<PeopleListRequest>
{
    private readonly IPeopleService peopleService = peopleService;

    /// <summary>
    /// Consumes the PeopleListRequest message and responds with a PeopleListResponse.
    /// </summary>
    /// <param name="context">The context of the consumed message.</param>
    public async Task Consume(ConsumeContext<PeopleListRequest> context)
    {
        var listPeople = await peopleService.GetListItemAsync();

        await context.RespondAsync(new PeopleListResponse { People = listPeople });
    }
}
