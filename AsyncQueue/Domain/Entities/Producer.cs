using Domain.Abstractions;

namespace Domain.Entities;

public class Producer: IEntity<Guid>
{
    public Guid Id { get; set; }
    public int CurrentSequenceNumber { get; set; }
    public bool IsDeleted { get; set; }
}