using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using System.Collections;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;


class Program
{
  private static readonly string?
  _serviceName = Environment.GetEnvironmentVariable("service2ContainerName")
  , _port = Environment.GetEnvironmentVariable("service2Port")
  , _url = "http://" + _serviceName + ":" + _port
  , _rabbitTopic = Environment.GetEnvironmentVariable("rabbitLogTopic")
  , _queueName = Environment.GetEnvironmentVariable("rabbitMessagesQueue");
  private static IModel? rabbitChannel;

  public static void Main()
  {
    Thread.Sleep(2000);  // sleep 2s to create some "lag" as instructed
    rabbitChannel = ConnectToRabbit();
    DeclareRabbitQueue();
    var consumer = CreateRabbitConsumer();
    startRabbitConsuming(consumer);
    var host = CreateWebHost();
    AppDomain.CurrentDomain.ProcessExit += async (sender, e) =>
    {
      await host.StopAsync();
    };
    host.Run();
  }

  private static async Task<string> GetValueByKeyFromRequestBody
    (HttpContext context, string key)
  {
    string text = "";
    try
    {
      var body = await new StreamReader(context.Request.Body)
        .ReadToEndAsync();
      var parsedBody = JsonSerializer.Deserialize<JsonElement>(body);
      text = parsedBody.GetProperty(key).ToString();
    }
    catch (Exception e)
    {
      Console.WriteLine($"error: {e.Message}");
    }
    return text;
  }

  private static string CreateLogText(HttpContext context, string text)
  {
    string address = context.Connection.RemoteIpAddress.ToString();
    string port = context.Connection.RemotePort.ToString();
    return text + " " + address + ":" + port;
  }

  private static IModel ConnectToRabbit()
  {
    string? _rabbitmqName =
    Environment.GetEnvironmentVariable("rabbitmqContainerName");
    var factory = new ConnectionFactory { HostName = _rabbitmqName, Port = 5672 };
    var connection = factory.CreateConnection();
    IModel channel = connection.CreateModel();
    return channel;
  }

  private static void DeclareRabbitQueue()
  {
    rabbitChannel.QueueDeclare(
      queue: _queueName,
      durable: false,
      exclusive: false,
      autoDelete: false);
  }

  private static EventingBasicConsumer CreateRabbitConsumer()
  {
    var consumer = new EventingBasicConsumer(rabbitChannel);
    consumer.Received += (model, ea) =>
    {
      var body = ea.Body.ToArray();
      string msg = Encoding.UTF8.GetString(body);
      HandleRabbitConsuming(msg);
    };
    return consumer;
  }

  private static void HandleRabbitConsuming(string msg)
  {
    var text = msg + " MSG";
    SendToRabbit(_rabbitTopic, "hi.log", text);
  }

  private static void startRabbitConsuming(EventingBasicConsumer consumer)
  {
    rabbitChannel.BasicConsume(
      queue: _queueName,
      autoAck: true,
      consumer: consumer);
  }

  private static void SendToRabbit(string topic, string key, string msg)
  {
    var body = Encoding.UTF8.GetBytes(msg);
    rabbitChannel.BasicPublish(exchange: topic, routingKey: key, body: body);
  }

  private static IWebHost CreateWebHost()
  {
    var host = new WebHostBuilder()
      .UseKestrel()
      .UseUrls(_url)
      .Configure(app =>
      {
        app.Run(async (context) =>
        {
          string text = await GetValueByKeyFromRequestBody(context, "text");
          text = CreateLogText(context, text);
          SendToRabbit(_rabbitTopic, "hi.log", text);
        });
      })
      .Build();
    return host;
  }

}
