namespace Infra;

public class Constants
{
    public const int SiloPort = 11111;
    public const int GatewayPort = 30000;
    public const string ClusterId = "LocalTestCluster";
    public const string ServiceId = "SFaaS";

    public const string KafkaService = "localhost:9092";
    public const string ZooKeeperService = "localhost:2181";

    public const string RedisPrimary = "localhost:6379";
    public const string RedisSecondary = "localhost:6380";
}

