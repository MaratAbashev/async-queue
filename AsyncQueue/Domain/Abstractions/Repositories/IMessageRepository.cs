using Domain.Entities;

namespace Domain.Abstractions.Repositories;

public interface IMessageRepository: IRepository<Message,Guid>
{
}