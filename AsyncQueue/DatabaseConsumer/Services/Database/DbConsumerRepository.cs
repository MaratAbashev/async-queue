using DatabaseConsumer.Models.Entities;

namespace DatabaseConsumer.Services.Database;

public class DbConsumerRepository(ConsumerDbContext dbContext) : IDbConsumerRepository
{
    public async Task AddMessageAsync(string message, CancellationToken cancellationToken)
    {
        await dbContext.Messages.AddAsync(new MessageEntity
        {
            Date = DateTime.UtcNow,
            Content = message
        }, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}