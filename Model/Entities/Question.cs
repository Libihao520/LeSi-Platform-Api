using Model.Common;

public class Question : Base
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
    public string ExampleInput { get; set; } = string.Empty;
    public string ExampleOutput { get; set; } = string.Empty;

    public int Difficulty { get; set; } = 1;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}