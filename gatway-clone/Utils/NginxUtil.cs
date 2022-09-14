using Org.BouncyCastle.Tsp;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace gatway_clone.Utils;

public class NginxUtil
{
    private static readonly ILogger _logger = LogUtil.CreateLogger();
    public static async Task Join()
    {
        if (!Configuration.IsUseNginxPlus) return;
        string request = JsonSerializer.Serialize(Configuration.NginxPlusCurrentNodeInfo, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
        foreach (string url in Configuration.NginxPlusUrls)
        {
            //if ((await GetServers(url)).Any(x => x.Server == Configuration.NginxPlusCurrentNodeInfo.Server))
            //{
            //    _logger.LogInformation($"{Configuration.NginxPlusCurrentNodeInfo.Server} already joined {url}");
            //    continue;
            //}
            //_logger.LogInformation($"Join nginx request: {request}");
            //string response = await HttpUtil.SendAsync(url, request, TimeSpan.FromSeconds(10), HttpMethod.Post);
            //_logger.LogInformation($"Join nginx response: {response}");
        }
    }
    //private static async Task<NginxServerInfo[]> GetServers(string url)
    //{
    //    //string response = await HttpUtil.SendAsync($"{url}", null, TimeSpan.FromSeconds(10), HttpMethod.Get);
    //    //_logger.LogInformation($"{url}: {response}");
    //    //return JsonSerializer.Deserialize<NginxServerInfo[]>(response);
    //}
    public class NginxServerInfo
    {
        [JsonPropertyName("id")]
        public int? ID { get; set; }
        [JsonPropertyName("server")]
        public string Server { get; set; }
        [JsonPropertyName("weight")]
        public int? Weight { get; set; }
        [JsonPropertyName("max_conns")]
        public int? MaxConnections { get; set; }
        [JsonPropertyName("max_fails")]
        public int? MaxFails { get; set; }
        [JsonPropertyName("fail_timeout")]
        public string FailTimeout { get; set; }
        [JsonPropertyName("slow_start")]
        public string SlowStart { get; set; }
        [JsonPropertyName("route")]
        public string Route { get; set; }
        [JsonPropertyName("backup")]
        public bool? Backup { get; set; }
        [JsonPropertyName("down")]
        public bool? Down { get; set; }
    }
}
