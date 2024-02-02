using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var factory = new ConnectionFactory
{
    HostName = "rabbitmq",
    UserName = "user",
    Password = "password"
};

using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.QueueDeclare("rpc_queue",
    false,
    false,
    false,
    null);
channel.BasicQos(0, 1, false);
var consumer = new EventingBasicConsumer(channel);
channel.BasicConsume("rpc_queue",
    false,
    consumer);

consumer.Received += (model, ea) =>
{
    var response = string.Empty;

    var body = ea.Body.ToArray();
    var props = ea.BasicProperties;
    var replyProps = channel.CreateBasicProperties();
    replyProps.CorrelationId = props.CorrelationId;

    try
    {
        var message = Encoding.UTF8.GetString(body);
        var n = ulong.Parse(message);
        Console.WriteLine($" [.] Fib({message})");
        response = Fib(n).ToString();
    }
    catch (Exception e)
    {
        Console.WriteLine($" [.] {e.Message}");
        response = string.Empty;
    }
    finally
    {
        var responseBytes = Encoding.UTF8.GetBytes(response);
        channel.BasicPublish(string.Empty,
            props.ReplyTo,
            replyProps,
            responseBytes);
        channel.BasicAck(ea.DeliveryTag, false);
    }
};

await Task.Delay(Timeout.Infinite);

// Assumes only valid positive integer input.
// Don't expect this one to work for big numbers, and it's probably the slowest recursive implementation possible.
static ulong Fib(ulong n)
{
    if (n < 2)
        return n;
    return Fib(n - 1) + Fib(n - 2);
}