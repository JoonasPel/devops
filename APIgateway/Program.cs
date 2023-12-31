using System.Text;
using System.Globalization;
using Newtonsoft.Json;
using System.Net.Http.Headers;

public class Program
{
  // env variables
  private static readonly string?
    _logsQueue = Environment.GetEnvironmentVariable("rabbitLogsQueue")
  , _messagesQueue = Environment.GetEnvironmentVariable("rabbitMessagesQueue")
  , _rabbitContainer = Environment.GetEnvironmentVariable("rabbitmqContainerName")
  , _rabbitManagementPort = Environment.GetEnvironmentVariable("rabbitManagementPort")
  , _rabbitBaseUrl = "http://" + _rabbitContainer + ":" + _rabbitManagementPort + "/api"
  , _monitorContainer = Environment.GetEnvironmentVariable("monitorContainerName")
  , _monitorPort = Environment.GetEnvironmentVariable("monitorPort")
  , _monitorUrl = "http://" + _monitorContainer + ":" + _monitorPort
  , _service1Container = Environment.GetEnvironmentVariable("service1ContainerName")
  , _service1Port = Environment.GetEnvironmentVariable("service1Port")
  , _service1Url = "http://" + _service1Container + ":" + _service1Port;

  private static HashSet<string> validStates = new HashSet<string>()
  {
    "INIT", "RUNNING", "PAUSED"
  };
  private static string currentState = "INIT";
  private static List<string> stateHistory = new List<string>();

  public static void Main()
  {
    var builder = WebApplication.CreateBuilder();
    builder.WebHost.UseKestrel(options => { options.ListenAnyIP(8083); });
    var app = builder.Build();
    HttpClient client = new HttpClient();

    app.MapGet("/messages", async () =>
    {
      try
      {
        using HttpResponseMessage response = await client.GetAsync(_monitorUrl);
        response.EnsureSuccessStatusCode();
        string body = await response.Content.ReadAsStringAsync();
        return body;
      }
      catch
      {
        return "error getting logs from monitor\n";
      }
    });

    app.MapPut("/state", async (HttpContext context) =>
    {
      try
      {
        string newState = await new StreamReader(
          context.Request.Body).ReadToEndAsync();
        if (validStates.Contains(newState))
        {
          UpdateState(newState);
          using HttpResponseMessage response = await client.PutAsync(_service1Url,
            new StringContent(newState, Encoding.UTF8, "text/plain"));
          response.EnsureSuccessStatusCode();
          /* if we send INIT to service1, it inits and then returns its new 
          state (RUNNING) so we change it here to make sure we have right
          state history in run-log. If state didn't change, no duplicate
          will be created. e.g. run-log (PAUSED -> PAUSED) doesn't happen */
          string state = await response.Content.ReadAsStringAsync();
          if (validStates.Contains(state))
          {
            UpdateState(state);
          }
        }
      }
      catch
      {
        return "error changing state";
      }
      return "";
    });

    app.MapGet("/state", () => currentState);

    app.MapGet("/run-log", () =>
    {
      return string.Join('\n', stateHistory);
    });

    // Gets overall and queue statistics from rabbitmq and returns a new
    // JSON (as string) that contains desired statistics.
    app.MapGet("/mqstatistic", async () =>
    {
      try
      {
        string authHeaderValue = Convert.ToBase64String(
          Encoding.ASCII.GetBytes("guest:guest"));
        client.DefaultRequestHeaders.Authorization =
          new AuthenticationHeaderValue("Basic", authHeaderValue);
        string url = _rabbitBaseUrl + "/overview";
        using HttpResponseMessage response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        string body = await response.Content.ReadAsStringAsync();
        dynamic jsonOverall = JsonConvert.DeserializeObject(body);
        url = _rabbitBaseUrl + "/queues/%2F/" + _messagesQueue;
        using HttpResponseMessage response2 = await client.GetAsync(url);
        response2.EnsureSuccessStatusCode();
        body = await response2.Content.ReadAsStringAsync();
        dynamic jsonQueue1 = JsonConvert.DeserializeObject(body);
        url = _rabbitBaseUrl + "/queues/%2F/" + _logsQueue;
        using HttpResponseMessage response3 = await client.GetAsync(url);
        response3.EnsureSuccessStatusCode();
        body = await response3.Content.ReadAsStringAsync();
        dynamic jsonQueue2 = JsonConvert.DeserializeObject(body);
        return CreateStatistics(jsonOverall, jsonQueue1, jsonQueue2);
      }
      catch
      {
        return "Error. Try Again!";
      }
    });

    app.Run();
  }

  // Updates the current state and saves state history.
  // If given same state than already is, does nothing.
  private static void UpdateState(string newState)
  {
    if (newState != currentState)
    {
      string currentDateTime = DateTime.UtcNow.ToString(
        "yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
      stateHistory.Add($"{currentDateTime}: {currentState}->{newState}");
      currentState = newState;
    }
  }

  // Takes dynamic JSONs generated by Newtonsoft and creates a new string
  // that contains statistics in JSON format
  private static string CreateStatistics(dynamic json1, dynamic json2,
    dynamic json3)
  {
    object overallStatistics = new
    {
      consumersOnline = json1.object_totals.consumers,
      queuesOnline = json1.object_totals.queues,
      messagePublishRate = json1.message_stats.publish_details.rate,
      messageDeliveryRate = json1.message_stats.deliver_get_details.rate,
      messagesDeliveredRecently = json1.message_stats.deliver_get,
    };
    object messagesQueueStatistics = new
    {
      messageDeliveryRate = json2.message_stats.deliver_get_details.rate,
      messagePublishRate = json2.message_stats.publish_details.rate,
      messagesDeliveredRecently = json2.message_stats.deliver_get,
      messagesPublishedRecently = json2.message_stats.publish,
    };
    object logsQueueStatistics = new
    {
      messageDeliveryRate = json3.message_stats.deliver_get_details.rate,
      messagePublishRate = json3.message_stats.publish_details.rate,
      messagesDeliveredRecently = json3.message_stats.deliver_get,
      messagesPublishedRecently = json3.message_stats.publish,
    };
    object totalData = new
    {
      overallStatistics,
      messagesQueueStatistics,
      logsQueueStatistics,
    };
    return JsonConvert.SerializeObject(totalData);
  }

}
