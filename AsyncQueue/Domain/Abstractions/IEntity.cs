namespace Domain.Abstractions;

public interface IEntity<TId> where TId: struct
{
    public TId Id { get; set; }
    public bool IsDeleted { get; set; }
}