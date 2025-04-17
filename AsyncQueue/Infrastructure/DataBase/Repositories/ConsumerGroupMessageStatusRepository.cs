using Domain.Abstractions.Repositories;
using Domain.Entities;

namespace Infrastructure.DataBase.Repositories;

public class ConsumerGroupMessageStatusRepository(BrokerDbContext context) :
    Repository<ConsumerGroupMessageStatus, long>(context),
    IConsumerGroupMessageStatusRepository;