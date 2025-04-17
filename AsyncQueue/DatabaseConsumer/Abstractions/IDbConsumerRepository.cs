namespace DatabaseConsumer.Abstractions;

public interface IDbConsumerRepository
{
    Task AddMessageAsync(string message, CancellationToken cancellationToken);
}