using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Model.Dto.AiModel;
using Model.Options;
using Model.Other;

namespace WebApi.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]/[action]")]
public class AigcController : ControllerBase
{
    private readonly IAiGcService _aiGcService;
    private readonly JWTTokenOptions _jwtTokenOptions;

    public AigcController(IAiGcService aiGcService, IOptionsMonitor<JWTTokenOptions> jwtTokenOptions)
    {
        _aiGcService = aiGcService;
        _jwtTokenOptions = jwtTokenOptions.CurrentValue;
    }

    [HttpGet]
    public Task<ApiResult> GetModelService([FromQuery] GetModelReq req)
    {
        return _aiGcService.GetModelService(req);
    }

    [HttpPost]
    [RequestFormLimits(MultipartBodyLengthLimit = 262144000)] // 设置表单数据的最大长度为250MB
    [RequestSizeLimit(262144000)] // 设置整个HTTP请求的最大大小为250MB
    public async Task<ApiResult> PutModelService([FromForm] PutModelReq req)
    {
        return await _aiGcService.PutModelService(req);
    }

    [HttpDelete]
    public Task<ApiResult> DelModelService(long id)
    {
        return _aiGcService.DelModelService(id);
    }

    /// <summary>
    ///  在线问答
    /// </summary>
    /// <param name="q"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpGet]
    public Task<ApiResult> QuestionsAndAnswers([FromQuery] string q, string? model, CancellationToken cancellationToken)
    {
        return _aiGcService.QuestionsAndAnswers(q, model, cancellationToken);
    }

    /// <summary>
    /// 在线问答（流式传输）
    /// </summary>
    /// <param name="q"></param>
    /// <param name="model"></param>
    [HttpGet]
    [AllowAnonymous]
    public async Task QuestionsAndAnswersStream([FromQuery] string q, string token, string model,
        CancellationToken cancellationToken)
    {
        // 验证 token 并获取 ClaimsPrincipal
        var principal = ValidateToken(token);
        if (principal == null)
        {
            Response.StatusCode = StatusCodes.Status401Unauthorized;
            await Response.WriteAsync("Unauthorized");
            return;
        }

        // 将 ClaimsPrincipal 设置到当前 HttpContext.User
        HttpContext.User = principal;

        var response = Response;
        response.Headers.Add("Content-Type", "text/event-stream");

        await foreach (var message in _aiGcService.QuestionsAndAnswersStream(q, model, cancellationToken))
        {
            // SSE 的消息格式是 "data: <message>\n\n"
            await response.WriteAsync($"data: {message}\n\n");
            await response.Body.FlushAsync(); // 确保消息被立即发送
        }
    }

    /// <summary>
    /// 获取缓存
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public ApiResult GetHistory()
    {
        return _aiGcService.GetHistoryService();
    }

    /// <summary>
    /// 移除缓存
    /// </summary>
    /// <returns></returns>
    [HttpDelete]
    public ApiResult DelHistory()
    {
        return _aiGcService.DelHistoryService();
    }

    // 手动验证 token 的方法
    private ClaimsPrincipal? ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtTokenOptions.SecurityKey);

        try
        {
            //验证 token 并返回 ClaimsPrincipal
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _jwtTokenOptions.Issuer,
                ValidAudience = _jwtTokenOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(key)
            }, out SecurityToken validatedToken);

            return principal; // 返回 ClaimsPrincipal
        }
        catch
        {
            return null; // token 无效
        }
    }
}