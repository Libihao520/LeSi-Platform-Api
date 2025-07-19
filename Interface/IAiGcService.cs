using Model.Dto.AiModel;
using Model.Dto.TestPaperManage;
using Model.Other;

namespace Interface;

public interface IAiGcService
{
    /// <summary>
    /// 查询所有模型
    /// </summary>
    /// <returns></returns>
    Task<ApiResult> GetModelService(GetModelReq req);

    /// <summary>
    /// 上传模型
    /// </summary>
    /// <returns></returns>
    Task<ApiResult> PutModelService(PutModelReq req);

    /// <summary>
    /// 删除模型
    /// </summary>
    /// <returns></returns>
    Task<ApiResult> DelModelService(long id);

    /// <summary>
    /// 在线问答
    /// </summary>
    /// <param name="q"></param>
    /// <returns></returns>
    Task<ApiResult> QuestionsAndAnswers(string q, string? model, CancellationToken cancellationToken);

    /// <summary>
    /// 在线问答（流式传输）
    /// </summary>
    /// <param name="q"></param>
    /// <returns></returns>
    IAsyncEnumerable<string> QuestionsAndAnswersStream(string q, string? model, CancellationToken cancellationToken);

    /// <summary>
    /// 获取缓存
    /// </summary>
    /// <returns></returns>
    ApiResult GetHistoryService();

    /// <summary>
    /// 移除缓存
    /// </summary>
    /// <returns></returns>
    ApiResult DelHistoryService();
}