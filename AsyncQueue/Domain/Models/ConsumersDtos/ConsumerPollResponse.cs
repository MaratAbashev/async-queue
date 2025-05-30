﻿namespace Domain.Models.ConsumersDtos;

public class ConsumerPollResponse
{
    public ProcessingStatus ProcessingStatus { get; set; }
    public string? Message { get; set; }
    public int PartitionId { get; set; }
    public int Offset { get; set; }
    public string? ValueType { get; set; }
    public List<ConsumerMessage>? ConsumerMessages { get; set; }
}