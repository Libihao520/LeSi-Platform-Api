using Newtonsoft.Json;

namespace Model.UtilData.StreamResponse
{
    public class SparkResponseStreamData
    {
        [JsonProperty("code")] public int Code { get; set; }

        [JsonProperty("message")] public string Message { get; set; }

        [JsonProperty("sid")] public string Sid { get; set; }

        [JsonProperty("choices")] public List<Choice> Choices { get; set; }

        [JsonProperty("usage")] public Usage Usage { get; set; }
    }

    public class Choice
    {
        [JsonProperty("delta")] public Message Delta { get; set; }

        [JsonProperty("index")] public int Index { get; set; }
    }

    public class Message
    {
        [JsonProperty("role")] public string Role { get; set; }

        [JsonProperty("content")] public string Content { get; set; }
    }

    public class Usage
    {
        [JsonProperty("prompt_tokens")] public int PromptTokens { get; set; }

        [JsonProperty("completion_tokens")] public int CompletionTokens { get; set; }

        [JsonProperty("total_tokens")] public int TotalTokens { get; set; }
    }
}