using System;
using System.Text;
using RabbitMQ.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RabbitMQTests
{
    [TestClass]
    public class RabbitMQTests
    {
        private const string RabbitMQHost = "localhost"; // RabbitMQ server hostname
        private const string QueueName = "test_queue";
        private ConnectionFactory factory = new ConnectionFactory() { 
            HostName = RabbitMQHost,
            Port = 5672, // RabbitMQ default port
            UserName = "username",
            Password = "password"
        };

        [TestMethod]
        public void CanConnectToRabbitMQ()
        {
            try
            {
                using (var connection = factory.CreateConnection())
                {
                    // If we reached here, the connection was successful
                    Assert.IsTrue(connection.IsOpen);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to RabbitMQ: {ex.Message}");
                Assert.Fail($"Failed to connect to RabbitMQ: {ex.Message}");
            }
        }

        [TestMethod]
        public void CanSendMessage()
        {
            try
            {
                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    // Declare the queue
                    channel.QueueDeclare(queue: QueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

                    // Publish a message
                    var message = "Hello, RabbitMQ!";
                    var body = Encoding.UTF8.GetBytes(message);
                    channel.BasicPublish(exchange: "", routingKey: QueueName, basicProperties: null, body: body);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message to RabbitMQ: {ex.Message}");
                Assert.Fail($"Failed to send message to RabbitMQ: {ex.Message}");
            }
        }

        [TestMethod]
        public void CanReceiveMessage()
        {
            try
            {
                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    // Declare the queue
                    channel.QueueDeclare(queue: QueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

                    // Create a consumer
                    var consumer = new RabbitMQ.Client.Events.EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        Console.WriteLine($"Received message: {message}");
                    };

                    // Start consuming messages
                    channel.BasicConsume(queue: QueueName, autoAck: true, consumer: consumer);

                    // Sleep to allow time for message consumption (you may need to adjust this)
                    System.Threading.Thread.Sleep(5000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiving message from RabbitMQ: {ex.Message}");
                Assert.Fail($"Failed to receive message from RabbitMQ: {ex.Message}");
            }
        }
    }
}
