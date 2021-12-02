using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Business
{
    public class Program{

        private static int Count { get; set; }

        public static void Main(string[] args)
        {
            try
            {
                var factory = new ConnectionFactory() { HostName = "172.17.0.2", UserName = "guest", Password = "guest" };

            Console.WriteLine("Creating connection");
            using (var connection = factory.CreateConnection())
            {
                Console.WriteLine("Connection open");
                Console.WriteLine("Creating model");
                using (var channel = connection.CreateModel())
                {
                    Console.WriteLine("Model created");

                    
                        channel.QueueDeclare(queue: "request", true, false, true);

                        channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
                    
                    Console.WriteLine(" [*] Waiting for messages.");

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body.ToArray();
                        var props = ea.BasicProperties;
                        var replyProps = channel.CreateBasicProperties();
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
                                channel.BasicPublish(exchange: "", routingKey: props.ReplyTo, basicProperties: replyProps, body: Encoding.UTF8.GetBytes(Count.ToString()));
                                break;
                            default:
                                throw new Exception("Invalid message");
                        }

                        Console.WriteLine("Message " + message + " handled");

                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    };
                    channel.BasicConsume(queue: "request", autoAck: false, consumer: consumer);

                    Console.WriteLine(" Press [enter] to exit.");
                    Console.ReadLine();
                }
            }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
