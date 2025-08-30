namespace Interface;

public interface ICodeExecutor
{
    Task<ExecutionResult> ExecuteJava(string code, string input);
    Task<ExecutionResult> ExecutePython(string code, string input);
}

public class ExecutionResult
{
    public bool Success { get; set; }
    public string Output { get; set; }
    public string Error { get; set; }
    public TimeSpan ExecutionTime { get; set; }
}