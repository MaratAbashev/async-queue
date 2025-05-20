using Application.Services;
using Domain.Abstractions.Repositories;
using Domain.Abstractions.Services;
using Domain.Models.ConsumersDtos;
using Domain.Models.ProducersDtos;
using Infrastructure.DataBase;
using Infrastructure.DataBase.Options;
using Infrastructure.DataBase.Repositories;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using IConsumerService = Domain.Abstractions.Services.IConsumerService;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

var logger = new LoggerConfiguration()
    .Enrich.WithEnvironmentName()
    .Enrich.WithProcessId()
    .Enrich.WithThreadId()
    .WriteTo.Console()
    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(builder.Configuration["ELASTICSEARCH_HOSTS"]!))
    {
        AutoRegisterTemplate = true,
        IndexFormat = "broker-api-logs-{0:yyyy.MM.dd}"
    })
    .CreateLogger();

Log.Logger = logger;

builder.Services.Configure<BrokerStartingData>(builder.Configuration.GetSection(nameof(BrokerStartingData)));

builder.Services.AddDbContext<BrokerDbContext>();
builder.Services.AddScoped<IProducerService, ProducerService>();
builder.Services.AddScoped<IConsumerService, ConsumerService>();
builder.Services.AddScoped<ITopicService, TopicService>();
builder.Services.AddScoped<IPartitionService, PartitionService>();
builder.Services.AddScoped<IProducerRepository, ProducerRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IConsumerGroupRepository, ConsumerGroupRepository>();
builder.Services.AddScoped<ITopicRepository, TopicRepository>();
builder.Services.AddScoped<IConsumerGroupMessageStatusRepository, ConsumerGroupMessageStatusRepository>();
builder.Services.AddScoped<IConsumerGroupOffsetRepository, ConsumerGroupOffsetRepository>();
builder.Services.AddScoped<IConsumerRepository, ConsumerRepository>();
builder.Services.AddScoped<IPartitionRepository, PartitionRepository>();

builder.Services.AddHostedService<DbInitializerService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Host.UseSerilog();

var app = builder.Build();
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json",
        $"{builder.Environment.ApplicationName} v1"));
}

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

var topicEndpointGroup = app.MapGroup("/topic");

topicEndpointGroup.MapPost("/add/{topicName}", 
    async (string topicName, ITopicService topicService) => 
        await topicService.AddNewTopic(topicName));
topicEndpointGroup.MapDelete("/delete/{topicName}", 
    async (string topicName, ITopicService topicService) => 
        await topicService.RemoveTopic(topicName) ? Results.Ok() : Results.Problem());

var partitionEndpointGroup = app.MapGroup("/partition");

partitionEndpointGroup.MapPost("/add/{topicName}", 
    async (string topicName, IPartitionService partitionService) => 
        await partitionService.AddPartition(topicName));
partitionEndpointGroup.MapDelete("/delete/{partitionId:int}", 
    async (int partitionId, IPartitionService partitionService) => 
        await partitionService.DeletePartition(partitionId));

await app.RunAsync();

app.Run();