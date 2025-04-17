using Domain.Entities;

namespace Domain.Abstractions.Repositories;

public interface IMessageRepository: IRepository<Message,Guid>
{
    Task<Dictionary<int,IEnumerable<Message>>> GroupMessagesInTopicByPartitionAsync(string topic);
}