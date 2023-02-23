namespace Orchestrator.Library;

public class HttpClientService
{
    private static readonly Lazy<HttpClientService> lazy =
        new Lazy<HttpClientService>(() => new HttpClientService());

    public static HttpClientService Instance { get { return lazy.Value; } }

    private HttpClient _client;

    private HttpClientService()
    {
        this._client = new HttpClient();
    }

    public async Task<HttpResponseMessage> SendHttpGet(string url)
    {
        return await this._client.GetAsync(url);
    }
    
    public async Task<HttpResponseMessage> SendHttpPost(string url, HttpContent content)
    {
        return await this._client.PostAsync(url, content);
    }
}