using Confluent.Kafka;
using Orleans.Concurrency;

namespace Infra.Kafka;

[Reentrant]
public class MediatorGrain : Grain, IMediatorGrain
{
    private IConsumer<string, string> _consumer;

    private string _topic;
    private int _partition;
    private int _offset;

    private CancellationToken cancellationToken;

    private IProducer<Null, Event> producer;
    private TopicPartition topicPartition;

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.cancellationToken = cancellationToken;
        return Task.CompletedTask;
    }

    public Task Init(string Topic, string GroupId, int Partition, int Offset)
    {
        this._topic = Topic;
        this._partition = Partition;
        this._offset = Offset;
        var consumerConfig = new ConsumerConfig
    	{
            GroupId = GroupId,
            BootstrapServers = Constants.KafkaService,
            EnableAutoCommit = false
    	};

    	this._consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
        var topicPartitionOffset = 
                  new TopicPartitionOffset(this._topic, new Partition(this._partition), new Offset(this._offset)); 
        this._consumer.Assign(topicPartitionOffset);

    	Task.Run(() => StartConsuming(this.cancellationToken), this.cancellationToken);

        var producerConfig = new ProducerConfig {
            BootstrapServers = Constants.KafkaService,
            AllowAutoCreateTopics = true
        };
        var kafkaBuilder = new ProducerBuilder<Null, Event>(producerConfig).SetValueSerializer(new EventSerializer());

        this.producer = kafkaBuilder.Build();

        // TODO Define topic partition
        this.topicPartition = new TopicPartition("", new Partition(-1));

        return Task.CompletedTask;
    }

    public async Task<bool> StartWorklow(string functionName, object[] parameters)
	{
        // TODO Assemble function name and parameters into an event object
        var @event = new Event();
        var res = await this.producer.ProduceAsync(this.topicPartition, new Message<Null, Event>
        {
            Timestamp = new Timestamp(Timestamp.UnixTimeEpoch, TimestampType.CreateTime),
            Value = @event
        });
        return res.Status == PersistenceStatus.Persisted;
    }

    private async Task StartConsuming(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = this._consumer.Consume(cancellationToken);
                    await ProcessMessageAsync(consumeResult.Message.Key, consumeResult.Message.Value);
                }
                catch (ConsumeException e)
                {
                    Console.WriteLine($"Consume error: {e.Error.Reason}");
                }
            }
        }
        catch (Exception){}
        finally
        {
            this._consumer.Close();
        }
    }

    private static Task ProcessMessageAsync(string key, string value)
    {
        // TODO Handle the Kafka message. In particular, trigger an executor grain, receive the function output, assemble it into an event, and publish it to the correct Kafka topic/partition. Make sure to acknowledge the processing of Kafka message correctly in order to ensure exactly-once processing.
        Console.WriteLine($"Received message: Key = {key}, Value = {value}");
        throw new NotImplementedException();
    }

    public override Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
    {
        this._consumer.Dispose();
        return Task.CompletedTask;
    }
}

