using Domain.Abstractions.Repositories;
using Domain.Entities;

namespace Infrastructure.DataBase.Repositories;

public class ConsumerRepository(BrokerDbContext context) :
    Repository<Consumer, Guid>(context),
    IConsumerRepository
{
    
}