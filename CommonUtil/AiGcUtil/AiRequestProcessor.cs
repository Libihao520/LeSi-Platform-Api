using System.Text;
using CommonUtil.RedisUtil;
using Microsoft.AspNetCore.Http;
using Model.Consts;
using Model.UtilData;

namespace CommonUtil.AiGcUtil;

/// <summary>
/// AI请求策略-上下文类
/// </summary>
public class AiRequestProcessor
{
    private readonly AiRequestStrategyFactory _requestStrategyFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserInformationUtil _informationUtil;

    public AiRequestProcessor(AiRequestStrategyFactory requestStrategyFactory, IHttpContextAccessor httpContextAccessor,
        UserInformationUtil informationUtil)
    {
        _requestStrategyFactory = requestStrategyFactory;
        _httpContextAccessor = httpContextAccessor;
        _informationUtil = informationUtil;
    }

    public async Task<string> SparkProcess(string q, CancellationToken cancellationToken)
    {
        var messages = GetAndUpdateMessages(q);
        var sparkRequestData = new AiRequestData { messages = messages };

        var requestStrategy = _requestStrategyFactory.Create("Spark");
        var requestAsync = await requestStrategy.RequestAsync(sparkRequestData, cancellationToken);

        GetAndUpdateMessages(requestAsync, "gpt");
        return requestAsync;
    }

    public async IAsyncEnumerable<string> SparkProcessStreamAsync(string q, CancellationToken cancellationToken)
    {
        var messages = GetAndUpdateMessages(q);
        var sparkRequestData = new AiRequestData
        {
            messages = messages,
            stream = true
        };
        var requestStrategy = _requestStrategyFactory.Create("Spark");
        var fullResponse = new StringBuilder(); // 用于累积完整响应

        await foreach (var chunk in requestStrategy.RequestStreamAsync<AiRequestData>(sparkRequestData,
                           cancellationToken))
        {
            yield return chunk;
            fullResponse.Append(chunk);
        }

        // 流结束后处理
        string finalResponse = fullResponse.ToString().Replace("\\n", "\n");
        GetAndUpdateMessages(finalResponse, "gpt"); // 存入缓存
    }

    public async Task<string> DeepSeekProcess(string q,string model, CancellationToken cancellationToken)
    {
        var messages = GetAndUpdateMessages(q);
        var sparkRequestData = new AiRequestData
        {
            model = model,
            messages = messages,
            stream = false
        };

        var requestStrategy = _requestStrategyFactory.Create("DeepSeek");
        var requestAsync = await requestStrategy.RequestAsync<AiRequestData>(sparkRequestData, cancellationToken);

        GetAndUpdateMessages(requestAsync, "gpt");
        return requestAsync;
    }

    public async IAsyncEnumerable<string> DeepSeekProcessStreamAsync(string q,string model, CancellationToken cancellationToken)
    {
        var messages = GetAndUpdateMessages(q);
        var requestData = new AiRequestData
        {
            model = model,
            messages = messages,
            stream = true
        };
        var requestStrategy = _requestStrategyFactory.Create("DeepSeek");
        var fullResponse = new StringBuilder(); // 用于累积完整响应

        await foreach (var chunk in requestStrategy.RequestStreamAsync<AiRequestData>(requestData,
                           cancellationToken))
        {
            yield return chunk;
            fullResponse.Append(chunk);
        }

        // 流结束后处理
        string finalResponse = fullResponse.ToString().Replace("\\n", "\n");
        GetAndUpdateMessages(finalResponse, "gpt"); // 存入缓存
    }
    private List<message> GetAndUpdateMessages(string newMessage, string role = "user")
    {
        var userId = _informationUtil.GetCurrentUserId();
        var cacheKey = string.Format(RedisKey.UserAiRecentDialogs, userId);

        var Messages = CacheManager.Exist(cacheKey)
            ? CacheManager.Get<List<message>>(cacheKey)
            : new List<message>();

        Messages.Add(new message() { role = role, content = newMessage });
        CacheManager.Set(cacheKey, Messages, TimeSpan.FromMinutes(30));

        return Messages.Where(m => m.role == role).ToList();
    }
}