using Domain.Entities;
using Domain.Models;

namespace Domain.Abstractions.Repositories;

public interface IConsumerGroupMessageStatusRepository: IRepository<ConsumerGroupMessageStatus,long>
{
    public Task<List<ConsumerGroupMessageStatus>> GetAllByStatusAndConsumerGroupIdAsync(int consumerGroupId,
        MessageStatus status);
    public Task UpdateConsumerGroupMessageStatusAsync(IEnumerable<ConsumerGroupMessageStatus> consumerGroupMessageStatuses,
        MessageStatus messageStatus);
}