using Domain.Abstractions.Repositories;
using Domain.Entities;

namespace Infrastructure.DataBase.Repositories;

public class ProducerRepository(BrokerDbContext context): 
    Repository<Producer, Guid>(context), 
    IProducerRepository;