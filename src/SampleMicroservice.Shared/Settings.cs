namespace SampleMicroservice.Shared;

public class Settings
{
    public const string RabbitMQHost = "localhost";
    public const string RabbitMQVirtualHost = "/";
    public const string RabbitMQUsername = "guest";
    public const string RabbitMQPassword = "guest";

    // Queue names
    public const string QueueNameRequest = "requestPeople";   // People list request queue
    public const string QueueNameResponse = "responsePeople"; // (unused by RPC reply-to ephemeral queue pattern)
    public const string QueueNamePerson = "queue.person";   // Person by id request queue (centralized here)

    public const string ExchangeType = "fanout";
    public const string DatabaseName = "People";
    public const string SwaggerTitle = "Sample API";
    public const string SwaggerVersion = "v1";

    public const bool Durable = true;
    public const bool AutoDelete = false;
    public const int PrefetchCount = 5;

    // Retry & timeout settings
    public const int RetryCount = 3;
    public const int RetryInterval = 5000; // milliseconds
    public const double QueueExpiration = 5;
}