namespace Model.Dto.CodeExercises;

public class CodeSubmissionRequest
{
    public long QuestionId { get; set; }
    public string Code { get; set; } = string.Empty;
    public bool IsPractice { get; set; } = false;
}