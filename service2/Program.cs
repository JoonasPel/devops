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
  private static readonly string _logFileName = "service2.log";
  private static readonly string _stopSignal = "STOP";

  public static void Main() {
    CreateOrWriteToLogFile(createOnly: true, filename: _logFileName);
    Thread.Sleep(2000);

    var host = new WebHostBuilder()
      .UseKestrel()
      .UseUrls(_url)
      .Configure(app => {
        app.Run(async (context) => {
          string text = await GetValueByKeyFromRequestBody(context, "text");
          if (text.Equals(_stopSignal)) {
            ExitApp(app);
          } else {
            text = CreateLogText(context, text);  
            CreateOrWriteToLogFile(
              createOnly: false, filename: _logFileName, textToWrite: text);
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
  
  private static void CreateOrWriteToLogFile
  (bool createOnly, string filename, string textToWrite = "") {
    try {
      string exePath = System.Reflection.Assembly.GetEntryAssembly().Location;
      string exeDir = Path.GetDirectoryName(exePath);
      bool append = !createOnly;
      using StreamWriter writer = new(exeDir + "/logs/" + filename, append);
      if (append) {
        writer.WriteLine(textToWrite);
      }
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
