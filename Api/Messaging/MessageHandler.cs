using System.Collections.Concurrent;

namespace Api
{
    public class MessageHandler : IMessageHandler
    {
        private readonly IConnection connection;
        private readonly IModel channel;
        private readonly EventingBasicConsumer consumer;
        private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> pendingMessages;

        private const string requestQueueName = "request";
        private const string replyQueueName = "reply";

        public MessageHandler()
        {
            pendingMessages = new ConcurrentDictionary<string,TaskCompletionSource<string>>();

            var factory = new ConnectionFactory() { HostName = "rabbitmq" };

            connection = factory.CreateConnection();
            channel = connection.CreateModel();

            channel.QueueDeclare(queue: requestQueueName,
                                    durable: false,
                                    exclusive: false,
                                    autoDelete: false,
                                    arguments: null);
            channel.QueueDeclare(queue: replyQueueName,
                                    durable: false,
                                    exclusive: false,
                                    autoDelete: false,
                                    arguments: null);

            consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                if (!pendingMessages.TryRemove(ea.BasicProperties.CorrelationId, out TaskCompletionSource<string>? tcs))
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

        public void Send(string message, IBasicProperties? props = null)
        {
            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(
                exchange: "", 
                routingKey: requestQueueName, 
                basicProperties: props, 
                body: body);

            Console.WriteLine("Message: \"" + message + "\" sent");
        }

        public Task<string> SendAndReceive(string message)
        {
            IBasicProperties props = channel.CreateBasicProperties();
            var correlationId = Guid.NewGuid().ToString();
            props.CorrelationId = correlationId;
            props.ReplyTo = replyQueueName;

            var body = Encoding.UTF8.GetBytes(message);
            var tcs = new TaskCompletionSource<string>();
            pendingMessages.TryAdd(correlationId, tcs);

            Send(message, props);

            return tcs.Task;
        }
    }
}
