namespace WebAPI.Backend.Core.Consumers;

/// <summary>
/// Consumer for PersonRequest messages.
/// </summary>
/// <remarks>
/// This consumer is responsible for handling incoming PersonRequest messages,
/// retrieving the corresponding person data, and responding with a PersonResponse message.
/// </remarks>
public class ConsumerPersonRequest(IPeopleService peopleService) : IConsumer<PersonRequest>
{
    private readonly IPeopleService peopleService = peopleService;

    /// <summary>
    /// Consumes the incoming PersonRequest message.
    /// </summary>
    /// <param name="context">The context of the consumed message.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task Consume(ConsumeContext<PersonRequest> context)
    {
        var personId = context.Message.Id;
        var person = await peopleService.GetItemAsync(personId);

        await context.RespondAsync(new PersonResponse { Person = person });
    }
}
