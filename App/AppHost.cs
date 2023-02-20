

using AirlineSendAgent.Client;
using AirlineSendAgent.Data;
using AirlineSendAgent.Dtos;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace AirlineSendAgent.App
{
    public class AppHost : IAppHost
    {
        private SendAgentDbContext _sendAgentDbContext;
        private IWebhookClient _webhookClient;

        public AppHost(SendAgentDbContext sendAgentDbContext, IWebhookClient webhookClient)
        {
            _sendAgentDbContext = sendAgentDbContext;
            _webhookClient = webhookClient;
        }
        public void Run()
        {
            //We need to register a listener event
            var factory = new ConnectionFactory() {HostName = "localhost", Port=5672};
            using(var connection = factory.CreateConnection())
            using(var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "trigger", type: ExchangeType.Fanout);

                var queueName = channel.QueueDeclare().QueueName;

                channel.QueueBind(queue: queueName, exchange: "trigger", routingKey: "");

                //Listening on message bus
                var consumer= new EventingBasicConsumer(channel);

                Console.WriteLine("Listening on Mesage bus...");

                consumer.Received += async(ModuleHandle, ea) => 
                {
                    Console.WriteLine("Event is triggered!");

                    //deserialize the message 
                    var body = ea.Body;
                    var notificationMessage = Encoding.UTF8.GetString(body.ToArray());
                    var message = JsonSerializer.Deserialize<NotificationMessageDto>(notificationMessage);

                    var webhookToSend = new FlightDetailChangePayloadDto()
                    {
                        WebhookType = message.WebhookType,
                        WebhookURI = string.Empty,
                        Secret = string.Empty,
                        Publisher = string.Empty,
                        OldPrice = message.OldPrice,
                        NewPrice = message.NewPrice,
                        FlightCode = message.FlightCode
                    };

                    foreach(var whs in _sendAgentDbContext.webhookSubscriptions.Where(subs=> subs.WebhookType.Equals(message.WebhookType))) 
                    {
                        webhookToSend.WebhookURI = whs.WebhookURI;
                        webhookToSend.Secret = whs.Secret;
                        webhookToSend.Publisher = whs.WebhookPublisher;

                        await _webhookClient.SendWebhookNotification(webhookToSend);
                    }
                };
                
                //pool/consume the message from the queue
                channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
                Console.ReadLine();

            }

        }
    }
}