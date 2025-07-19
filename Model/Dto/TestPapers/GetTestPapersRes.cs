namespace Model.Dto.TestPapers;

/// <summary>
/// 获取题目
/// </summary>
public class GetTestPapersRes
{
    /// <summary>
    /// 单选题
    /// </summary>
    public List<SingleChoice>? SingleChoice { get; set; }

    /// <summary>
    /// 多选题
    /// </summary>
    public List<MultipleChoice>? MultipleChoice { get; set; }

    /// <summary>
    /// 判断题
    /// </summary>
    public List<TrueFalse>? TrueFalse { get; set; }
}

public class SingleChoice
{
    public long Id { get; set; }
    public string? Title { get; set; }

    public List<string>? Options { get; set; }

    public int TopicNumber { get; set; }

    public string? Answer { get; set; }

    public string? SubAnswer { get; set; }
}

public class MultipleChoice
{
    public long Id { get; set; }
    public string? Title { get; set; }
    public List<string>? Options { get; set; }

    public int TopicNumber { get; set; }

    public string? Answer { get; set; }

    public string? SubAnswer { get; set; }
}

public class TrueFalse
{
    public long Id { get; set; }
    public string? Title { get; set; }
    public List<string>? Options { get; set; }

    public int TopicNumber { get; set; }

    public string? Answer { get; set; }

    public string? SubAnswer { get; set; }
}