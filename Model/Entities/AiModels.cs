using Model.Common;

namespace Model.Entities;

public class AiModels : Entity
{
    /// <summary>
    /// 模型地址
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    /// 模型类型
    /// </summary>
    public string? ModelCls { get; set; }

    /// <summary>
    /// 模型名称
    /// </summary>
    public string? ModelName { get; set; }

    /// <summary>
    /// 模型大小
    /// </summary>
    public float? ModelSize { get; set; }
}