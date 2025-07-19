namespace Model.UtilData;

public class DeepSeekResponseData
{
    public string Id { get; set; }
    public string Object { get; set; }
    public long Created { get; set; }
    public string Model { get; set; }
    public List<Choice> Choices { get; set; }
    public Usage Usage { get; set; }
    public object PromptLogprobs { get; set; } 
}

public class Choice
{
    public int Index { get; set; }
    public Message Message { get; set; }
    public object Logprobs { get; set; } 
    public string FinishReason { get; set; }
    public object StopReason { get; set; } 
}

public class Message
{
    public string Role { get; set; }
    public string Content { get; set; }
    public string ReasoningContent { get; set; }
    public List<object> ToolCalls { get; set; }
}

public class Usage
{
    public int PromptTokens { get; set; }
    public int TotalTokens { get; set; }
    public int CompletionTokens { get; set; }
}