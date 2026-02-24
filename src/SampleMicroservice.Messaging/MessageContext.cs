namespace SampleMicroservice.Messaging;

/// <summary>
/// Provides a logical call-context holder for a correlation identifier used across asynchronous
/// and synchronous execution flows within the microservice messaging components.
/// </summary>
/// <remarks>
/// The correlation identifier is stored in an <see cref="System.Threading.AsyncLocal{T}"/>,
/// which ensures the value flows with asynchronous calls but remains isolated per logical
/// execution context. Use this to attach a request or operation identifier to outgoing
/// messages and logs so related operations can be correlated.
/// </remarks>
public static class MessageContext
{
    /// <summary>
    /// Async-local storage for the current correlation identifier.
    /// </summary>
    /// <remarks>
    /// This field is private and readonly; access should be performed through the
    /// <see cref="CorrelationId"/> property. The stored value may be <c>null</c>.
    /// </remarks>
    private static readonly AsyncLocal<string?> correlationId = new();

    /// <summary>
    /// Gets or sets the current correlation identifier for the logical execution context.
    /// </summary>
    /// <value>
    /// A string representing the correlation identifier, or <c>null</c> when none is set.
    /// </value>
    /// <remarks>
    /// Setting this property updates the async-local storage so the identifier is available
    /// to downstream asynchronous operations executed within the same logical context.
    /// </remarks>
    public static string? CorrelationId
    {
        get => correlationId.Value;
        set => correlationId.Value = value;
    }

    /// <summary>
    /// Clears the current correlation identifier from the logical execution context.
    /// </summary>
    /// <remarks>
    /// After calling <see cref="Clear"/>, <see cref="CorrelationId"/> will return <c>null</c>.
    /// Use this when the correlation identifier should not be propagated further.
    /// </remarks>
    public static void Clear() => correlationId.Value = null;
}