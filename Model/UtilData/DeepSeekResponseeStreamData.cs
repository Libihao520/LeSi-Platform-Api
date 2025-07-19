namespace Model.UtilData;

using Newtonsoft.Json;

public class DeepSeekResponseStreamData
{
    [JsonProperty("id")] public string Id { get; set; }

    [JsonProperty("object")] public string Object { get; set; }

    [JsonProperty("created")] public long Created { get; set; }

    [JsonProperty("model")] public string Model { get; set; }

    [JsonProperty("choices")] public List<DeepSeekChoice> Choices { get; set; }

    [JsonProperty("usage")] public DeepSeekUsage Usage { get; set; }
}

public class DeepSeekChoice
{
    [JsonProperty("index")] public int Index { get; set; }

    [JsonProperty("delta")] public DeepSeekDelta Delta { get; set; }

    [JsonProperty("logprobs")] public object Logprobs { get; set; }

    [JsonProperty("finish_reason")] public string FinishReason { get; set; }
}

public class DeepSeekDelta
{
    [JsonProperty("reasoning_content")] public string ReasoningContent { get; set; }
    [JsonProperty("content")] public string Content { get; set; }
}

public class DeepSeekUsage
{
    [JsonProperty("prompt_tokens")] public int PromptTokens { get; set; }

    [JsonProperty("total_tokens")] public int TotalTokens { get; set; }

    [JsonProperty("completion_tokens")] public int CompletionTokens { get; set; }
}