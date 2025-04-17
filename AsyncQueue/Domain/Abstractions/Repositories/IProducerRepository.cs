using Domain.Entities;
using Domain.Models;

namespace Domain.Abstractions.Repositories;

public interface IProducerRepository: IRepository<Producer,Guid> {}