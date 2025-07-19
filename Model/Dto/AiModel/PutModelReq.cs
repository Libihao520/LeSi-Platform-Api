using Microsoft.AspNetCore.Http;

namespace Model.Dto.AiModel;

public class PutModelReq
{
    /// <summary>
    /// 主键
    /// </summary>
    public long? Id { get; set; }

    /// <summary>
    /// 模型类型
    /// </summary>
    public string? ModelCls { get; set; }

    /// <summary>
    /// 模型名称
    /// </summary>
    public string? ModelName { get; set; }

    /// <summary>
    /// 模型
    /// </summary>
    // public IFormFile Model { get; set; }
}