using SampleMicroservice.Messaging;
using WebAPI.Backend.Core.Handlers;

namespace WebAPI.Backend.HostedServices;

public class RabbitResponderHostedService(IRabbitMqBus bus, ILogger<RabbitResponderHostedService> logger, IServiceProvider serviceProvider) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting RabbitResponderHostedService...");
        bus.Start();

        // Register responders that resolve handlers from DI (dependency injection) scope when message arrives
        bus.RegisterResponder<PeopleListRequest, PeopleListResponse>(Settings.QueueNameRequest, async request =>
        {
            // MessageContext.CorrelationId è impostata dal RabbitMqBus prima di chiamare il handler
            var corr = MessageContext.CorrelationId;
            logger.LogInformation("Handler invoked PeopleListRequest CorrelationId={CorrelationId}", corr);

            using var scope = serviceProvider.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<PeopleListRequestHandler>();

            var response = await handler.HandleAsync(request);

            logger.LogInformation("Handler completed PeopleListRequest CorrelationId={CorrelationId} items={Count}", corr, response?.People?.Count ?? 0);
            return response;
        });

        bus.RegisterResponder<PersonRequest, PersonResponse>(Settings.QueueNamePerson, async request =>
        {
            var corr = MessageContext.CorrelationId;
            logger.LogInformation("Handler invoked PersonRequest Id={Id} CorrelationId={CorrelationId}", request?.Id, corr);

            using var scope = serviceProvider.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<PersonRequestHandler>();

            var response = await handler.HandleAsync(request);

            logger.LogInformation("Handler completed PersonRequest Id={Id} CorrelationId={CorrelationId} found={HasPerson}", request?.Id, corr, response?.Person != null);
            return response;
        });

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopping RabbitResponderHostedService...");
        bus.Stop();

        return Task.CompletedTask;
    }
}