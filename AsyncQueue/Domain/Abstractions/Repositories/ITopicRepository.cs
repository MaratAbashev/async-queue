using Domain.Entities;

namespace Domain.Abstractions.Repositories;

public interface ITopicRepository: IRepository<Topic,int>
{
    Task<Dictionary<int,IEnumerable<Message>>> GroupMessagesInTopicByPartitionAsync(string topic);
}