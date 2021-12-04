using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Business
{
    public class Worker : BackgroundService
    {
        private IConnection _connection;
        private IModel _channel;

        private static int Count { get; set; }

        public Worker()
        {
            var factory = new ConnectionFactory() { HostName = "rabbitmq" };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(queue: "request",
                                    durable: false,
                                    exclusive: false,
                                    autoDelete: false,
                                    arguments: null);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += OnReceived;

            _channel.BasicConsume(queue: "request", autoAck: false, consumer: consumer);

            Console.WriteLine("Worker started");
            return;
        }

        private void OnReceived(object ch, BasicDeliverEventArgs ea)
        {
            var body = ea.Body.ToArray();
            var props = ea.BasicProperties;
            var replyProps = _channel.CreateBasicProperties();
            replyProps.CorrelationId = props.CorrelationId;

            var message = Encoding.UTF8.GetString(body);

            switch (message)
            {
                case "increment":
                    Count++;
                    break;
                case "decrement":
                    Count--;
                    break;
                case "get":
                    _channel.BasicPublish(exchange: "", routingKey: props.ReplyTo, basicProperties: replyProps, body: Encoding.UTF8.GetBytes(Count.ToString()));
                    break;
                default:
                    throw new Exception("Invalid message");
            }

            Console.WriteLine("Message \"" + message + "\" handled, count is " + Count);

            _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
        }
    }
}