using ClassLibrary.EFCore.Interfaces;

namespace SampleMicroservice.Shared.Entity;

public class PersonEntity : IEntity<int>
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public string Cognome { get; set; }
    public string Nome { get; set; }
    public string Email { get; set; }
}