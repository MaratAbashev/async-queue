namespace DatabaseConsumer.Models;

public record MessageResult<T>(T Value, int PartitionId, long Offset); // ВНИМАНИЕ!!! АЙДИ ПАРТИЦИИ INT