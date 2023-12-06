using System.Text;
using System.Globalization;

public class Program
{
  private static HashSet<string> validStates = new HashSet<string>(){
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
        using HttpResponseMessage response = await client.GetAsync(
          "http://monitorjoonaspelttari:8087");
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
          using HttpResponseMessage response = await client.PutAsync(
            "http://service1joonaspelttari:3000", new StringContent(
              newState, Encoding.UTF8, "text/plain"));
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

}
