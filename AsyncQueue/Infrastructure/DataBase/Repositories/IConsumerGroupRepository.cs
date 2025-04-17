using Domain.Abstractions.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataBase.Repositories;

public class ConsumerGroupRepository(BrokerDbContext context) :
    Repository<ConsumerGroup, int>(context),
    IConsumerGroupRepository
{
}