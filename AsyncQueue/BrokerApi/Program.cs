using Application.Services;
using Domain.Abstractions.Repositories;
using Domain.Abstractions.Services;
using Domain.Entities;
using Domain.Models.ConsumersDtos;
using Domain.Models.ProducersDtos;
using Infrastructure.DataBase;
using Infrastructure.DataBase.Options;
using Infrastructure.DataBase.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using IConsumerService = Domain.Abstractions.Services.IConsumerService;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();
builder.Services.Configure<BrokerStartingData>(builder.Configuration.GetSection(nameof(BrokerStartingData)));

builder.Services.AddDbContext<BrokerDbContext>();
builder.Services.AddScoped<IProducerService, ProducerService>();
builder.Services.AddScoped<IConsumerService, ConsumerService>();
builder.Services.AddScoped<IProducerRepository, ProducerRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IConsumerGroupRepository, ConsumerGroupRepository>();
builder.Services.AddScoped<ITopicRepository, TopicRepository>();
builder.Services.AddScoped<IConsumerGroupMessageStatusRepository, ConsumerGroupMessageStatusRepository>();
builder.Services.AddScoped<IConsumerGroupOffsetRepository, ConsumerGroupOffsetRepository>();
builder.Services.AddScoped<IConsumerRepository, ConsumerRepository>();

builder.Services.AddHostedService<DbInitializerService>();

var app = builder.Build();

// var configuration = app.Configuration;
// using var scope = app.Services.CreateScope();
// var context = scope.ServiceProvider.GetRequiredService<BrokerDbContext>();
// var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
// try
// {
//     context.Database.Migrate();
//     // context.Topics.Add(new Topic
//     // {
//     //     TopicName = configuration["MessageBroker:Topic"],
//     // });
//     // context.SaveChanges();
//     // var topicId = context.Topics.First().Id;
//     // int.TryParse(configuration["MessageBroker:PartitionCount"], out int partitionCount);
//     // for (int i = 0; i < partitionCount; i++)
//     // {
//     //     context.Partitions.Add(new Partition
//     //     {
//     //         TopicId = topicId,
//     //     });
//     // }
//     //
//     // context.ConsumerGroups.Add(new ConsumerGroup
//     // {
//     //     TopicId = topicId,
//     //     ConsumerGroupName = configuration["MessageBroker:ConsumerGroup"],
//     // });
//     // context.SaveChanges();
//     // var partitionIds = context.Partitions.Select(p => p.Id).ToList();
//     // var consumerGroupId = context.ConsumerGroups.First().Id;
//     // foreach (var partitionId in partitionIds)
//     // {
//     //     context.ConsumerGroupOffsets.Add(new ConsumerGroupOffset
//     //     {
//     //         PartitionId = partitionId,
//     //         Offset = 0,
//     //         ConsumerGroupId = consumerGroupId
//     //     });
//     // }
//     //
//     // context.SaveChanges();
// }
// catch(Exception ex)
// {
//     logger.LogCritical(ex.Message + "qqqqq");
// }

var producerGroup = app.MapGroup("/producer");

producerGroup.MapPost("/register",
    async (ProducerRegistrationRequest request, IProducerService producerService) =>
        await producerService.RegisterAsync(request));

producerGroup.MapPost("/produce",
    async (ProducerSendRequest request, IProducerService producerService) =>
        await producerService.ProduceAsync(request));

var consumerEndpointGroup = app.MapGroup("/consumer");

consumerEndpointGroup.MapPost("/register", async (ConsumerRegisterRequest request, IConsumerService consumerService) => await consumerService.RegisterConsumerAsync(request));

consumerEndpointGroup.MapGet("/{consumerId}/{batchSize}/poll", async (Guid consumerId, int batchSize, IConsumerService consumerService) =>
{
    var result = await consumerService.Poll(consumerId, batchSize);
    return result.ConsumerMessages is { Count: 0 } ? // поработать лучше со статус кодами
        Results.NoContent() : Results.Ok(result);
});

consumerEndpointGroup.MapPost("/{consumerId}/commit", async (Guid consumerId, ConsumerCommitRequest request, IConsumerService consumerService) => await consumerService.TryCommitMessages(consumerId, request)?
    Results.Ok():
    Results.Conflict()); //поработать со статус кодами

app.Run();