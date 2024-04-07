using ClassLibrary.EFCore.Interfaces;

namespace SampleMicroservice.Shared.Entity;

/// <summary>
/// Represents a person entity in the system.
/// </summary>
public class PersonEntity : IEntity<int>
{
    /// <summary>
    /// Gets or sets the identifier of the person.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the user identifier associated with the person.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the surname of the person.
    /// </summary>
    public string Cognome { get; set; }

    /// <summary>
    /// Gets or sets the name of the person.
    /// </summary>
    public string Nome { get; set; }

    /// <summary>
    /// Gets or sets the email of the person.
    /// </summary>
    public string Email { get; set; }
}
