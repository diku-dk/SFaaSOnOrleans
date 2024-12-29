using Confluent.Kafka;

namespace Infra.Kafka;

public class ConsumerGrain : Grain, IConsumerGrain
{
	private IConsumer<string, string> _consumer;

    private readonly string _topic = "your-kafka-topic";
    private readonly int _partition = 0;
    private readonly int _offset = 0;

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
	{
    	  var config = new ConsumerConfig
    	  {
        	GroupId = "your-consumer-group",
        	BootstrapServers = "localhost:9092",
        	EnableAutoCommit = false
    	  };

    	  _consumer = new ConsumerBuilder<string, string>(config).Build();
        var topicPartitionOffset = 
                  new TopicPartitionOffset(_topic, 
                        new Partition(_partition), new Offset(_offset)); 
        _consumer.Assign(topicPartitionOffset);

    	_ = Task.Run(() => StartConsuming(cancellationToken), cancellationToken);

    	await base.OnActivateAsync(cancellationToken);
	}

	private async Task StartConsuming(CancellationToken cancellationToken)
	{
    	  try
    	  {
          while (!cancellationToken.IsCancellationRequested)
          {
            try
            {
               var consumeResult = _consumer.Consume(cancellationToken);
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
        	_consumer.Close();
    	  }
	}

	private static Task ProcessMessageAsync(string key, string value)
	{
    	  // Handle the Kafka message
    	  Console.WriteLine($"Received message: Key = {key}, Value = {value}");
    	  return Task.CompletedTask;
	}

	public override Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
	{
    	_consumer.Dispose();
        return Task.CompletedTask;
	}
}

