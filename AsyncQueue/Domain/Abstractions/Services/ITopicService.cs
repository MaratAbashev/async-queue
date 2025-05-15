using Domain.Entities;

namespace Domain.Abstractions.Services;

public interface ITopicService
{
    Task<Topic?> AddNewTopic(string topicName, CancellationToken cancellationToken = default);
    Task<bool> RemoveTopic(string topicName, CancellationToken cancellationToken = default);
}