var builder = WebApplication.CreateBuilder(args);
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

app.Run();
