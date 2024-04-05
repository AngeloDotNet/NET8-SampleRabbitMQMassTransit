using SampleMicroservice.Shared.Entity;

namespace SampleMicroservice.Shared.Models;

public class PeopleListResponse
{
    public List<PersonEntity> People { get; set; }
}