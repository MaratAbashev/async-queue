namespace DbConsumer.Abstractions;

public interface IDbConsumerRepository
{
    Task AddMessageAsync(string message, CancellationToken cancellationToken);
}