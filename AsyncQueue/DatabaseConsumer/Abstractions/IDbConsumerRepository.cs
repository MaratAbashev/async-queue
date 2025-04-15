namespace DatabaseConsumer.Services.Database;

public interface IDbConsumerRepository
{
    Task AddMessageAsync(string message, CancellationToken cancellationToken);
}