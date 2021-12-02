using System.Collections.Concurrent;

namespace Api
{
    public class MessageHandler
    {
        private readonly IConnection connection;
        private readonly IModel channel;
        private readonly string requestQueueName;
        private readonly string replyQueueName;
        private readonly EventingBasicConsumer consumer;
        private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> callbackMapper = new();

        public MessageHandler()
        {
            var factory = new ConnectionFactory() { HostName = "rabbitmq", UserName = "guest", Password = "guest" };

            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            // declare a server-named queue
            requestQueueName = channel.QueueDeclare(queue: "request", true, false, true).QueueName;
            replyQueueName = channel.QueueDeclare(queue: "reply", true, false, true).QueueName;
            consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                if (!callbackMapper.TryRemove(ea.BasicProperties.CorrelationId, out TaskCompletionSource<string>? tcs))
                    return;
                var body = ea.Body.ToArray();
                var response = Encoding.UTF8.GetString(body);
                tcs.TrySetResult(response);
            };

            channel.BasicConsume(
              consumer: consumer,
              queue: replyQueueName,
              autoAck: true);
        }

        public void Send(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: "", routingKey: requestQueueName, basicProperties: null, body: body);
            Console.WriteLine(" [x] Sent {0}", message);
        }

        public Task<string> SendAndReceive(string message, CancellationToken cancellationToken = default)
        {
            IBasicProperties props = channel.CreateBasicProperties();
            var correlationId = Guid.NewGuid().ToString();
            props.CorrelationId = correlationId;
            props.ReplyTo = replyQueueName;

            var body = Encoding.UTF8.GetBytes(message);
            var tcs = new TaskCompletionSource<string>();
            callbackMapper.TryAdd(correlationId, tcs);

            channel.BasicPublish(
                exchange: "",
                routingKey: requestQueueName,
                basicProperties: props,
                body: body);
            Console.WriteLine(" [x] Sent {0}", message);

            cancellationToken.Register(() => callbackMapper.TryRemove(correlationId, out var tmp));
            return tcs.Task;
        }
    }
}
