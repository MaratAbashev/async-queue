using Application.Services;
using BrokerApi.Filters;
using Domain.Abstractions.Repositories;
using Domain.Abstractions.Services;
using Domain.Models.ConsumersDtos;
using Domain.Models.ProducersDtos;
using Infrastructure.DataBase;
using Infrastructure.DataBase.Repositories;
using IConsumerService = Domain.Abstractions.Services.IConsumerService;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();
builder.Services.AddDbContext<BrokerDbContext>();
builder.Services.AddTransient<IStartupFilter, DataBaseStartUpFilter>();
builder.Services.AddScoped<IProducerService, ProducerService>();
builder.Services.AddScoped<IConsumerService, ConsumerService>();
builder.Services.AddScoped<IProducerRepository, ProducerRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IConsumerGroupRepository, ConsumerGroupRepository>();
builder.Services.AddScoped<ITopicRepository, TopicRepository>();
builder.Services.AddScoped<IConsumerGroupMessageStatusRepository, ConsumerGroupMessageStatusRepository>();
builder.Services.AddScoped<IConsumerGroupOffsetRepository, ConsumerGroupOffsetRepository>();
builder.Services.AddScoped<IConsumerRepository, ConsumerRepository>();

var app = builder.Build();

var producerGroup = app.MapGroup("/producer");

producerGroup.MapPost("/register",
    async (ProducerRegistrationRequest request, IProducerService producerService) =>
        await producerService.RegisterAsync(request));

producerGroup.MapPost("/produce",
    async (ProducerSendRequest request, IProducerService producerService) =>
        await producerService.ProduceAsync(request));

var consumerEndpointGroup = app.MapGroup("/consumer");

consumerEndpointGroup.MapPost("/register", async (ConsumerRegisterRequest request, IConsumerService consumerService) => await consumerService.RegisterConsumerAsync(request));

consumerEndpointGroup.MapGet("/{consumerId}/poll", async (Guid consumerId, IConsumerService consumerService) =>
{
    var result = await consumerService.Poll(consumerId);
    return result.ConsumerMessages is { Count: 0 } ? // поработать лучше со статус кодами
        Results.NoContent() : Results.Ok(result);
});

consumerEndpointGroup.MapPost("/{consumerId}/commit", async (Guid consumerId, ConsumerCommitRequest request, IConsumerService consumerService) => await consumerService.TryCommitMessages(consumerId, request)?
    Results.Ok():
    Results.Conflict()); //поработать со статус кодами

app.Run();