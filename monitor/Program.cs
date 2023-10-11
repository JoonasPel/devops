using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;


public class Program {
  private static readonly string?
  _port = Environment.GetEnvironmentVariable("monitorPort")
  , _containerName = Environment.GetEnvironmentVariable("monitorContainerName")
  , _url = "http://"+_containerName+":"+_port;
  private static IModel? rabbitChannel;
  private static List<string> _logTexts = new List<string>();

  public static void Main() {
    Thread.Sleep(20000); // todo wait-for-it.sh
    rabbitChannel = ConnectToRabbit();
    var consumer = CreateRabbitConsumer();
    StartRabbitConsuming(consumer);

    var host = CreateWebHost();
    AppDomain.CurrentDomain.ProcessExit += async (sender, e) => {
      await host.StopAsync();
    };
    host.Run();
  }

  private static IModel ConnectToRabbit() {
    string? _rabbitmqName =
    Environment.GetEnvironmentVariable("rabbitmqContainerName");
    var factory = new ConnectionFactory { HostName = _rabbitmqName, Port=5672 };
    var connection = factory.CreateConnection();
    IModel channel = connection.CreateModel();
    return channel;
  }

  private static EventingBasicConsumer CreateRabbitConsumer() {
    var consumer = new EventingBasicConsumer(rabbitChannel);
    consumer.Received += (model, ea) => {
      var body = ea.Body.ToArray();
      string msg = Encoding.UTF8.GetString(body);
      HandleRabbitConsuming(msg);
    };
    return consumer;
  }

  private static void StartRabbitConsuming(EventingBasicConsumer consumer) {
    string? queueName =
    Environment.GetEnvironmentVariable("rabbitLogsQueue");
    rabbitChannel.BasicConsume(
      queue: queueName,
      autoAck: true,
      consumer: consumer);
  }

  private static void HandleRabbitConsuming(string msg) {
    _logTexts.Add(msg);
  }

  private static IWebHost CreateWebHost() {
    var host = new WebHostBuilder()
      .UseKestrel()
      .UseUrls(_url)
      .Configure(app => {
        app.Run(async (context) => {
          context.Response.ContentType = "text/plain";
          var text = string.Join("\n", _logTexts);
          await context.Response.WriteAsync(text);
        });
      })
      .Build();
    return host;
  }
}
