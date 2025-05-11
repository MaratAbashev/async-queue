namespace Infrastructure.DataBase.Options;

public class BrokerStartingData
{
    public List<TopicConfig> Topics { get; set; }
}

public class TopicConfig
{
    public string TopicName { get; set; }
    public int PartitionCount { get; set; }
    public List<string> ConsumerGroups { get; set; }
}