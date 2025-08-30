namespace Model.Dto.CodeExecution;

public class CodeExecutionRequest
{
    public string Code { get; set; } = string.Empty;
    public string? Input { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
}