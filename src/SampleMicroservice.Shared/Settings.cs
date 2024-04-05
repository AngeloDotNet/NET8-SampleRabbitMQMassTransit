namespace SampleMicroservice.Shared;

public class Settings
{
    public const string RabbitMQHost = "localhost";
    public const string RabbitMQVirtualHost = "/";
    public const string RabbitMQUsername = "guest";
    public const string RabbitMQPassword = "guest";

    public const string QueueNameRequest = "requestPeople";
    public const string QueueNameResponse = "responsePeople";
    public const string ExchangeType = "fanout";

    public const string DatabaseName = "People";
    public const string SwaggerTitle = "Sample API";
    public const string SwaggerVersion = "v1";

    public const bool Durable = true; // default: true
    public const bool AutoDelete = false; // default: false

    public const int PrefetchCount = 5; //default: 16
    public const int RetryCount = 3;
    public const int RetryInterval = 5000;

    public const double QueueExpiration = 5;
}