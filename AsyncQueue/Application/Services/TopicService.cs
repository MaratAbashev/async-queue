using Domain.Abstractions.Repositories;
using Domain.Abstractions.Services;
using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class TopicService(ITopicRepository topicRepository,
    ILogger<TopicService> logger) : ITopicService
{
    public async Task<Topic?> AddNewTopic(string topicName, CancellationToken cancellationToken = default)
    {
        try
        {
            return await topicRepository.AddAsync(new Topic
            {
                TopicName = topicName
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return null;
        }
    }

    public async Task<bool> RemoveTopic(string topicName, CancellationToken cancellationToken = default)
    {
        try
        {
            await topicRepository.DeleteAsync(t => t.TopicName == topicName);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return false;
        }
    }
}