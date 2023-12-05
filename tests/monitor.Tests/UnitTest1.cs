//using System.Net.Http;
using NUnit.Framework;

[TestFixture]
public class Tests
{
    private HttpClient httpClient;
    private string url;

    [SetUp]
    public void Setup()
    {
        httpClient = new HttpClient();
        url = "http://localhost:8087"; // todo env var
    }

    [Test]
    public void Test1()
    {
        Assert.AreEqual(1, 1);
    }

    [Test]
    public void Test2()
    {
        Assert.Pass();
    }

}
