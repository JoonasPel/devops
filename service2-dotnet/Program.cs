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

  public static void Main() {
    var host = new WebHostBuilder()
      .UseKestrel()
      .UseUrls("http://"+_serviceName+":"+_port)
      .Configure(app => {
        app.Run(async (context) => {
          string text = await GetValueByKeyFromRequestBody(context, "text");
          //Console.WriteLine(text);
          if (text.Equals("STOP")) { 
            CloseApp(); 
          } else {
            WriteToLogFile(text);
          }
          await context.Response.WriteAsync("hello Joonas");
        });
      })
      .Build();

    host.Run();
  }

  private async static Task<string> GetValueByKeyFromRequestBody
    (HttpContext context, string key) {
    var body = await new StreamReader(context.Request.Body)
      .ReadToEndAsync();
    var parsedBody = JsonSerializer.Deserialize<JsonElement>(body);
    string text = "";
    try {
      text = parsedBody.GetProperty(key).ToString();
    } catch (Exception e) {
      Console.WriteLine($"error: {e.Message}");
    }
    return text;
  }

  private static void WriteToLogFile(string text) {

  }

  private static void CreateOrClearLogFile() {

  }

  private static void CloseApp() {
    Environment.Exit(5);
  }

}


