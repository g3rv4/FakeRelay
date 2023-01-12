using System.Net.Http.Headers;
using System.Runtime.Serialization;
using Jil;

namespace FakeRelay.Core.Helpers;

public static class GrafanaHelper
{
    private static HttpClient? _httpClient;
    private static HttpClient HttpClient => _httpClient ??= GetHttpClient();

    private static HttpClient GetHttpClient()
    {
        var httpClient = new HttpClient { BaseAddress = new Uri("https://" + Config.Instance.GrafanaHost) };
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Config.Instance.GrafanaKey);

        return httpClient;
    }
    
    public static async Task<int> GetCountInPeriod(string hostname, string period)
    {
        if (Config.Instance.GrafanaHost.IsNullOrEmpty() || Config.Instance.GrafanaKey.IsNullOrEmpty() || !Config.Instance.GrafanaDataSourceId.HasValue)
        {
            return -1;
        }

        var requestContent = new StringContent($@"
{{""queries"":[{{""datasourceId"":{Config.Instance.GrafanaDataSourceId.Value},""expr"":""increase(fr_index_requests{{exported_instance=\""{hostname}\""}}[{period}])""}}],""to"":""now"",""from"":""now""}}
    ");
        requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        var result = await HttpClient.PostAsync("/api/ds/query", requestContent);
        if (!result.IsSuccessStatusCode)
        {
            return -1;
        }

        var json = await result.Content.ReadAsStringAsync();
        var data = JSON.Deserialize<GrafanaResponse>(json, Options.CamelCase);
        return (int)Math.Round(data.Results.A.Frames[0].Data.Values[1][0]);
    }
}

public class GrafanaResponse
{
    public ResultData Results { get; set; }

    public class ResultData
    {
        [DataMember(Name = "A")]
        public AData A { get; set; }

        public class AData
        {
            public FrameData[] Frames { get; set; }

            public class FrameData
            {
                public DataData Data { get; set; }

                public class DataData
                {
                    public float[][] Values { get; set; }
                }
            }
        }
    }
}
