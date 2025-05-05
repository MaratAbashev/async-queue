using Application.Services;
using BrokerApi;
using Domain.Abstractions.Repositories;
using Domain.Abstractions.Services;
using Domain.Models.ConsumersDtos;
using Domain.Models.ProducersDtos;
using Infrastructure.DataBase;
using Infrastructure.DataBase.Repositories;
using IConsumerService = Domain.Abstractions.Services.IConsumerService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BrokerDbContext>();
builder.Services.AddScoped<DataBaseStartUp>();
builder.Services.AddScoped<IProducerService, ProducerService>();
builder.Services.AddScoped<IProducerRepository, ProducerRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IConsumerGroupRepository, ConsumerGroupRepository>();
builder.Services.AddScoped<ITopicRepository, TopicRepository>();
builder.Services.AddScoped<IConsumerGroupMessageStatusRepository, ConsumerGroupMessageStatusRepository>();

var app = builder.Build();

app.MapPost("/initialize", async (DataBaseStartUp startUp) => await startUp.Initialize());

app.MapPost("/producer/register",
    async (ProducerRegistrationRequest request, IProducerService producerService) =>
        await producerService.RegisterAsync(request));

app.MapPost("/producer/produce",
    async (ProducerSendRequest request, IProducerService producerService) =>
        await producerService.ProduceAsync(request));

var consumerEndpointGroup = app.MapGroup("/consumer");

consumerEndpointGroup.MapPost("/register", async (ConsumerRegisterRequest request, IConsumerService consumerService) =>
{
    await consumerService.RegisterConsumerAsync(request);
});

consumerEndpointGroup.MapGet("/{consumerId}/poll", async (Guid consumerId, IConsumerService consumerService) =>
{
    var result = await consumerService.Poll(consumerId);
    return result.ConsumerMessages is { Count: 0 } ? // поработать лучше со статус кодами
        Results.NoContent() : Results.Ok(result);
});

app.Run();