namespace DbConsumer.Models;

public record CommitContext(int PartitionId, long Offset); // ВНИМАНИЕ!!! АЙДИ ПАРТИЦИИ INT