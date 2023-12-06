using NUnit.Framework;
using System.Net;
using RestSharp;

[TestFixture]
public class TestRESTAPI
{
  private string baseUrl;
  private RestClient client;

  [SetUp]
  public void Setup()
  {
    baseUrl = "http://localhost:8083";
    client = new RestClient(baseUrl);
  }

  [Test]
  public void GETmessages()
  {
    RestRequest request = new RestRequest("/messages", Method.Get);
    RestResponse response = client.Execute(request);

    int status = (int)response.StatusCode;
    Assert.AreEqual(200, status, "statusCode check");

    string? contentType = response.ContentType;
    Assert.AreEqual("text/plain", contentType, "content-type check");

    if (response.Content != null)
    {
      string[] logs = response.Content.Split('\n');
      string firstLog = logs.FirstOrDefault("");
      Assert.IsTrue(firstLog.Contains('Z'),
        "log should contain Z (date ends with it)");
    }
    else
    {
      Assert.Fail("response didnt have a body");
    }
  }

  [Test]
  public void PUTandGETstate()
  {
    // Change state to PAUSED and check it worked
    RestRequest pauseRequest = new RestRequest("/state", Method.Put);
    pauseRequest.AddParameter("text/plain", "PAUSED", ParameterType.RequestBody);
    client.Execute(pauseRequest);
    Thread.Sleep(1000);  // give the system time to change its state
    RestRequest getStateRequest = new RestRequest("/state", Method.Get);
    RestResponse response = client.Execute(getStateRequest);
    Assert.AreEqual("PAUSED", response.Content);
    // Change state to PAUSED again and check it is still PAUSED
    client.Execute(pauseRequest);
    Thread.Sleep(1000);
    RestResponse response2 = client.Execute(getStateRequest);
    Assert.AreEqual("PAUSED", response2.Content);
    // Change state to RUNNING and check it worked
    RestRequest runningRequest = new RestRequest("/state", Method.Put);
    runningRequest.AddParameter("text/plain", "RUNNING", ParameterType.RequestBody);
    client.Execute(runningRequest);
    Thread.Sleep(1000);
    RestResponse response3 = client.Execute(getStateRequest);
    Assert.AreEqual("RUNNING", response3.Content);
    // Change state to INIT and check it is soon RUNNING because INIT changes
    // to RUNNING automatically!
    RestRequest initRequest = new RestRequest("/state", Method.Put);
    initRequest.AddParameter("text/plain", "INIT", ParameterType.RequestBody);
    client.Execute(initRequest);
    Thread.Sleep(1000);
    RestResponse response4 = client.Execute(getStateRequest);
    Assert.AreEqual("RUNNING", response4.Content);
  }

  [Test]
  public void GETrunlog()
  {
    Assert.Ignore("TEST NOT IMPLEMENTED");
  }

  [Test]
  public void GETmqstatistic()
  {
    Assert.Ignore("TEST NOT IMPLEMENTED");
  }
}

