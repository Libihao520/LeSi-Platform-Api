using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Model.Options;
using Model.UtilData;
using Model.UtilData.Response;
using Model.UtilData.StreamResponse;
using Newtonsoft.Json;

namespace CommonUtil.AiGcUtil;

public class DeepSeekAiRequestStrategy : IAiRequestStrategy
{
    private readonly List<AiGcService> _aiGcServices;
    private readonly ILogger<DeepSeekAiRequestStrategy> _logger;

    public DeepSeekAiRequestStrategy(IOptions<List<AiGcService>> aiGcOptions, ILogger<DeepSeekAiRequestStrategy> logger)
    {
        _aiGcServices = aiGcOptions.Value;
        _logger = logger;
    }

    public async Task<string> RequestAsync<T>(T q, CancellationToken cancellationToken)
    {
        using (var client = new HttpClient())
        {
            var modelProp = typeof(T).GetProperty("model");

            var aiGcService = _aiGcServices.FirstOrDefault(x => x.Model == modelProp.GetValue(q)?.ToString());
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {aiGcService.Token}");
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            var jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(q);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            try
            {
                var response = await client.PostAsync(aiGcService.Url, content, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var responseData = JsonConvert.DeserializeObject<DeepSeekResponseData>(responseString);
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
            var modelProp = typeof(T).GetProperty("model");

            var aiGcService = _aiGcServices.FirstOrDefault(x => x.Model == modelProp.GetValue(q)?.ToString());
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {aiGcService.Token}");
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            var jsonContent = new StringContent(JsonConvert.SerializeObject(q), Encoding.UTF8,
                "application/json");

            using (var request = new HttpRequestMessage(HttpMethod.Post, aiGcService.Url))
            {
                request.Content = jsonContent;

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

                                var responseData = JsonConvert.DeserializeObject<DeepSeekResponseStreamData>(line);
                                if (responseData != null && responseData.Choices != null &&
                                    responseData.Choices.Count > 0)
                                {
                                    var delta = responseData.Choices[0].Delta;
                                    var content = delta?.ReasoningContent ?? delta?.Content ?? string.Empty;
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