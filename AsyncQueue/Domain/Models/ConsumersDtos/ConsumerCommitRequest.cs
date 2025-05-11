namespace Domain.Models.ConsumersDtos;

public class ConsumerCommitRequest
{
    public int PartitionId { get; set; }
    public long Offset { get; set; }
    public int BatchSize { get; set; }
    public int? SuccessProcessedMessagesCount { get; set; }
}