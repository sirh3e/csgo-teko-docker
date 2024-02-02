using System.Collections.Concurrent;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Sirh3e.Teko.Docker.API;

public class RpcClient : IDisposable
{
    private const string QUEUE_NAME = "rpc_queue";
    private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> callbackMapper = new();
    private readonly IModel channel;

    private readonly IConnection connection;
    private readonly string replyQueueName;

    public RpcClient()
    {
        var factory = new ConnectionFactory
        {
            HostName = "rabbitmq",
            UserName = "user",
            Password = "password"
        };

        connection = factory.CreateConnection();
        channel = connection.CreateModel();
        // declare a server-named queue
        replyQueueName = channel.QueueDeclare().QueueName;
        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            if (!callbackMapper.TryRemove(ea.BasicProperties.CorrelationId, out var tcs))
                return;
            var body = ea.Body.ToArray();
            var response = Encoding.UTF8.GetString(body);
            tcs.TrySetResult(response);
        };

        channel.BasicConsume(consumer: consumer,
            queue: replyQueueName,
            autoAck: true);
    }

    public void Dispose()
    {
        channel.Close();
        connection.Close();
    }

    public Task<string> CallAsync(string message, CancellationToken cancellationToken = default)
    {
        var props = channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        props.ReplyTo = replyQueueName;
        var messageBytes = Encoding.UTF8.GetBytes(message);
        var tcs = new TaskCompletionSource<string>();
        callbackMapper.TryAdd(correlationId, tcs);

        channel.BasicPublish(string.Empty,
            QUEUE_NAME,
            props,
            messageBytes);

        cancellationToken.Register(() => callbackMapper.TryRemove(correlationId, out _));
        return tcs.Task;
    }
}