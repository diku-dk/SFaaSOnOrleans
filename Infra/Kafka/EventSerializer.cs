using Confluent.Kafka;
using MessagePack;

namespace Infra.Kafka;

public class EventSerializer : ISerializer<Event>, IDeserializer<Event>
{
    private readonly MessagePackSerializerOptions options = MessagePackSerializerOptions.Standard.WithResolver(MessagePack.Resolvers.ContractlessStandardResolver.Instance);

    public byte[] Serialize(Event e, SerializationContext _)
    {
        var data = MessagePackSerializer.Serialize(e, options);
        return data;
    }

    public Event Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext _)
    {
        if (isNull) {
                Console.WriteLine("Data received is null!");
            return null;
        }
        var e = MessagePackSerializer.Deserialize<Event>(data.ToArray(), options);
        return e;
    }
}

