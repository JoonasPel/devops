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
  public void PUTzstate()
  {
    Assert.Ignore("TEST NOT IMPLEMENTED");
  }

  [Test]
  public void GETstate()
  {
    Assert.Ignore("TEST NOT IMPLEMENTED");
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

