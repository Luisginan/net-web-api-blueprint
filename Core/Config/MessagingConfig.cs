using System.Diagnostics.CodeAnalysis;

namespace Core.Config;

[ExcludeFromCodeCoverage]
public class MessagingConfig
{
    public string SaslUsername { get; init; } = "";
    public string SaslPassword { get; init; } = "";
    public int SessionTimeoutMs { get; init; }
    public string BootstrapServers { get; init; } = "";

    public bool Authentication { get; init; }

    public string ProjectId { get; init; } = "";
    public string Provider { get; init; } = "pubsub";
    public string TopicSuffix { get; init; } = "";
    public string SenderId { get; init; } = "";
    
    public List<MessagingTopicConfig> Producer { get; init; } = new();
    public List<MessagingTopicConfig> Consumer { get; init; } = new();
    public int InitialCount { get; set; } = 100;
    public int MaxCount { get; set; } = 100;
}

[ExcludeFromCodeCoverage]
public class MessagingTopicConfig
{
    public string Name { get; init; } = "";
    public string TopicId { get; init; } = "";
    public string GroupId { get; init; } = "";
    public string DeadLetterGroupId { get; init; } = "";
    public string DeadLetterTopicId { get; init; } = "";
    public string Description { get; init; } = "";
}