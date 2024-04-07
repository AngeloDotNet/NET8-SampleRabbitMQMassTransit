namespace SampleMicroservice.Shared;

/// <summary>
/// This class contains the settings for the application.
/// </summary>
public class Settings
{
    /// <summary>
    /// The host for the RabbitMQ server.
    /// </summary>
    public const string RabbitMQHost = "localhost";

    /// <summary>
    /// The virtual host for the RabbitMQ server.
    /// </summary>
    public const string RabbitMQVirtualHost = "/";

    /// <summary>
    /// The username for the RabbitMQ server.
    /// </summary>
    public const string RabbitMQUsername = "guest";

    /// <summary>
    /// The password for the RabbitMQ server.
    /// </summary>
    public const string RabbitMQPassword = "guest";

    /// <summary>
    /// The name of the request queue.
    /// </summary>
    public const string QueueNameRequest = "requestPeople";

    /// <summary>
    /// The name of the response queue.
    /// </summary>
    public const string QueueNameResponse = "responsePeople";

    /// <summary>
    /// The type of exchange for the RabbitMQ server.
    /// </summary>
    public const string ExchangeType = "fanout";

    /// <summary>
    /// The name of the database.
    /// </summary>
    public const string DatabaseName = "People";

    /// <summary>
    /// The title for the Swagger documentation.
    /// </summary>
    public const string SwaggerTitle = "Sample API";

    /// <summary>
    /// The version for the Swagger documentation.
    /// </summary>
    public const string SwaggerVersion = "v1";

    /// <summary>
    /// The durability setting for the RabbitMQ server.
    /// </summary>
    public const bool Durable = true; // default: true

    /// <summary>
    /// The auto delete setting for the RabbitMQ server.
    /// </summary>
    public const bool AutoDelete = false; // default: false

    /// <summary>
    /// The prefetch count for the RabbitMQ server.
    /// </summary>
    public const int PrefetchCount = 5; //default: 16

    /// <summary>
    /// The retry count for the RabbitMQ server.
    /// </summary>
    public const int RetryCount = 3;

    /// <summary>
    /// The retry interval for the RabbitMQ server.
    /// </summary>
    public const int RetryInterval = 5000;

    /// <summary>
    /// The expiration time for the queue.
    /// </summary>
    public const double QueueExpiration = 5;
}