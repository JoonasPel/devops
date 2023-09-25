using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;


class Program {
  private static readonly string _serviceName = "service2joonaspelttari";
  private static readonly string _port = "8000";
  private static readonly string _url = "http://"+_serviceName+":"+_port;

  public static void Main() {
    CreateEmptyLogFile();
    Thread.Sleep(2000);
    var host = new WebHostBuilder()
      .UseKestrel()
      .UseUrls(_url)
      .Configure(app => {
        app.Run(async (context) => {
          string text = await GetValueByKeyFromRequestBody(context, "text");
          if (text.Equals("STOP")) {
            ExitApp(app);
          } else {
            text = CreateLogText(context, text);  
            WriteToLogFile(text);
          }
        });
      })
      .Build();

    host.Run();
  }

  private static async Task<string> GetValueByKeyFromRequestBody
    (HttpContext context, string key) {
    string text = "";
    try {
      var body = await new StreamReader(context.Request.Body)
        .ReadToEndAsync();
      var parsedBody = JsonSerializer.Deserialize<JsonElement>(body);  
      text = parsedBody.GetProperty(key).ToString();
    } catch (Exception e) {
      Console.WriteLine($"error: {e.Message}");
    }
    return text;
  }

  private static void WriteToLogFile(string text) {
    try {
      string exePath = System.Reflection.Assembly.GetEntryAssembly().Location;
      string exeDir = Path.GetDirectoryName(exePath);
      using StreamWriter writer = new(exeDir + "/logs/service2.log", true);
      writer.WriteLine(text);
      writer.Close();
    } catch (Exception e) {  
      Console.WriteLine("error creating log file");
    }
  }

  private static void CreateEmptyLogFile() {
    try {
      string exePath = System.Reflection.Assembly.GetEntryAssembly().Location;
      string exeDir = Path.GetDirectoryName(exePath);
      using StreamWriter writer = new(exeDir + "/logs/service2.log");
      writer.Close();
    } catch (Exception e) {  
      Console.WriteLine("error creating log file");
    }
  }

  private static string CreateLogText(HttpContext context, string text) {
    string address = context.Connection.RemoteIpAddress.ToString();
    string port = context.Connection.RemotePort.ToString();
    return text + " " + address + ":" + port;
  }

  private static void ExitApp(IApplicationBuilder app) {
    app.ApplicationServices.GetRequiredService
      <IHostApplicationLifetime>().StopApplication();
  }

}
