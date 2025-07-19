using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Model;
using Model.Options;
using Model.UtilData;
using Model.UtilData.Response;
using Model.UtilData.StreamResponse;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CommonUtil.AiGcUtil;

public class SparkAiRequestStrategy : IAiRequestStrategy
{
    private readonly AiGcService _aiGcService;
    private readonly ILogger<SparkAiRequestStrategy> _logger;

    public SparkAiRequestStrategy(IOptions<List<AiGcService>> aiGcOptions, ILogger<SparkAiRequestStrategy> logger)
    {
        _logger = logger;
        _aiGcService = aiGcOptions.Value.FirstOrDefault(x => x.Model == "Spark-Max");
    }

    public async Task<string> RequestAsync<T>(T q, CancellationToken cancellationToken)
    {
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _aiGcService.Token);
            client.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            var jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(q);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            try
            {
                var response = await client.PostAsync(_aiGcService.Url, content, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var responseData = JsonConvert.DeserializeObject<SparkResponseData>(responseString);
                    return responseData.Choices[0].Message.Content;
                }

                return "业务繁忙，请稍后再试！";
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("请求被取消！");
                return "";
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred: {ex.Message}");
                return "业务繁忙，请稍后再试！";
            }
        }
    }

    public async IAsyncEnumerable<string> RequestStreamAsync<T>(T q, CancellationToken cancellationToken)
    {
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _aiGcService.Token);
            client.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            var jsonContent = new StringContent(JsonConvert.SerializeObject(q), Encoding.UTF8,
                "application/json");

            // 创建 HttpRequestMessage
            using (var request = new HttpRequestMessage(HttpMethod.Post, _aiGcService.Url))
            {
                request.Content = jsonContent;

                // 发送请求并等待响应头 HttpCompletionOption.ResponseHeadersRead
                // 流式处理响应体 
                using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead,
                           cancellationToken))
                {
                    response.EnsureSuccessStatusCode();

                    using (var stream = await response.Content.ReadAsStreamAsync())
                    using (var reader = new StreamReader(stream))
                    {
                        while (!reader.EndOfStream)
                        {
                            if (cancellationToken.IsCancellationRequested)
                            {
                                _logger.LogInformation("请求被取消！");
                                yield break;
                            }

                            var line = await reader.ReadLineAsync();
                            if (!string.IsNullOrEmpty(line))
                            {
                                line = line.Substring("data: ".Length).Trim();
                                if (line == "[DONE]")
                                {
                                    yield break; // 结束流式传输
                                }

                                var responseData = JsonConvert.DeserializeObject<SparkResponseStreamData>(line);
                                if (responseData != null && responseData.Choices != null &&
                                    responseData.Choices.Count > 0)
                                {
                                    var content = responseData.Choices[0].Delta?.Content ?? string.Empty;
                                    content = content.Replace("\n", "\\n");
                                    yield return content;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}