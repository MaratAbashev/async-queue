using Application.Services;
using BrokerApi;
using Domain.Abstractions.Repositories;
using Domain.Abstractions.Services;
using Domain.Models.ProducersDtos;
using Infrastructure.DataBase;
using Infrastructure.DataBase.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BrokerDbContext>();
builder.Services.AddScoped<DataBaseStartUp>();
builder.Services.AddScoped<IProducerService, ProducerService>();
builder.Services.AddScoped<IProducerRepository, ProducerRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IConsumerGroupRepository, ConsumerGroupRepository>();
builder.Services.AddScoped<IConsumerGroupMessageStatusRepository, ConsumerGroupMessageStatusRepository>();

var app = builder.Build();

app.MapPost("/producer/register",
    async (ProducerRegistrationRequest request, IProducerService producerService) =>
        await producerService.RegisterAsync(request));

app.MapPost("/producer/produce",
    async (ProducerSendRequest request, IProducerService producerService) =>
        await producerService.ProduceAsync(request));

app.Run();