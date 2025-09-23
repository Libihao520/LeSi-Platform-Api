namespace Model.Dto.CodeExercises;

public class CodeSubmissionResult
{
    public bool IsCorrect { get; set; }
    public int Score { get; set; }
    public string Output { get; set; } = string.Empty;
    public string ExpectedOutput { get; set; } = string.Empty;
}