using System.Diagnostics;
using System.Text;
using CommonUtil;
using Interface;
using Microsoft.Extensions.Logging;
using Model;

namespace Service;

public class DockerCodeExecutor : ICodeExecutor, IDisposable
{
    private readonly string _tempBaseDir;
    private readonly List<string> _tempDirsToCleanup = new();
    private readonly UserInformationUtil _userInformationUtil;
    private readonly ILogger<DockerCodeExecutor> _logger;

    public DockerCodeExecutor(UserInformationUtil userInformationUtil, ILogger<DockerCodeExecutor> logger)
    {
        _userInformationUtil = userInformationUtil;
        _logger = logger;
        // 创建临时目录，每个用户一个目录
        _tempBaseDir = Path.Combine(Path.GetTempPath(), "code_executor",
            _userInformationUtil.GetCurrentUserId().ToString());
        Directory.CreateDirectory(_tempBaseDir);
    }

    public async Task<ExecutionResult> ExecuteJava(string code, string input)
    {
        string tempDir = null;
        try
        {
            // 1. 创建临时目录和文件
            tempDir = CreateTempDirectory();
            var javaFile = Path.Combine(tempDir, "Main.java");
            var inputFile = Path.Combine(tempDir, "input.txt");

            // 先移除 BOM 再写入文件
            var cleanedCode = RemoveUtf8Bom(code);
            await File.WriteAllTextAsync(javaFile, cleanedCode, new UTF8Encoding(false));
            await File.WriteAllTextAsync(inputFile, input, new UTF8Encoding(false));

            // 2. 检查文件是否确实创建
            if (!File.Exists(javaFile))
            {
                return new ExecutionResult
                {
                    Success = false,
                    Output = "",
                    Error = "无法创建 Java 文件"
                };
            }

            // 2. 使用安全的参数构建方式
            var volumeMount = $"{EscapePath(tempDir)}:/app";

            var processResult = await RunDockerCommandAsync(
                "run", "--rm", "-v", volumeMount,
                "registry.cn-heyuan.aliyuncs.com/libihao/jdk:8.0", "bash", "-c",
                "cd /app && javac Main.java && java Main < input.txt"
            );

            return new ExecutionResult
            {
                Success = processResult.ExitCode == 0,
                Output = processResult.Output,
                Error = processResult.Error,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "代码执行失败");
            return new ExecutionResult
            {
                Success = false,
                Output = "",
                Error = "代码执行失败: " + ex.Message,
            };
        }
        finally
        {
            // 3. 确保资源清理
            SafeDeleteDirectory(tempDir);
        }
    }

    public async Task<ExecutionResult> ExecutePython(string code, string input)
    {
        string tempDir = null;
        try
        {
            tempDir = CreateTempDirectory();
            var pyFile = Path.Combine(tempDir, "main.py");
            await File.WriteAllTextAsync(pyFile, code, Encoding.UTF8);

            var inputFile = Path.Combine(tempDir, "input.txt");
            await File.WriteAllTextAsync(inputFile, input, Encoding.UTF8);

            var volumeMount = $"{EscapePath(tempDir)}:/app";

            var processResult = await RunDockerCommandAsync(
                "run", "--rm", "-v", volumeMount,
                "python:3", "bash", "-c",
                "cd /app && python main.py < input.txt"
            );

            return new ExecutionResult
            {
                Output = processResult.Output,
                Error = processResult.Error,
            };
        }
        finally
        {
            SafeDeleteDirectory(tempDir);
        }
    }

    /// <summary>
    /// 移除 UTF-8 BOM 标记
    /// </summary>
    private string RemoveUtf8Bom(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        // UTF-8 BOM 字节序列: EF BB BF
        byte[] utf8Bom = new byte[] { 0xEF, 0xBB, 0xBF };
        var preamble = Encoding.UTF8.GetPreamble();

        if (preamble.Length >= 3 &&
            preamble[0] == utf8Bom[0] &&
            preamble[1] == utf8Bom[1] &&
            preamble[2] == utf8Bom[2])
        {
            // 如果文本以 BOM 开头，移除它
            if (text.Length > 0 && text[0] == '\uFEFF')
            {
                return text.Substring(1);
            }
        }

        return text;
    }

    /// <summary>
    /// 安全地执行 Docker 命令
    /// </summary>
    private async Task<(string Output, string Error, int ExitCode)> RunDockerCommandAsync(params string[] args)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = string.Join(" ", args.Select(EscapeArgument)),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        using var process = new Process { StartInfo = psi };

        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        process.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null) outputBuilder.AppendLine(e.Data);
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null) errorBuilder.AppendLine(e.Data);
        };

        process.Start();

        // 开始异步读取输出
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        // 设置超时（例如30秒）
        var timeout = TimeSpan.FromSeconds(30);
        var exited = await WaitForExitAsync(process, timeout);

        if (!exited)
        {
            process.Kill(true);
            return ("", $"Execution timeout after {timeout.TotalSeconds} seconds", -1);
        }

        await process.WaitForExitAsync();

        return (outputBuilder.ToString(), errorBuilder.ToString(), process.ExitCode);
    }

    private async Task<bool> WaitForExitAsync(Process process, TimeSpan timeout)
    {
        var waitTask = process.WaitForExitAsync();
        var timeoutTask = Task.Delay(timeout);

        var completedTask = await Task.WhenAny(waitTask, timeoutTask);
        return completedTask == waitTask;
    }

    /// <summary>
    /// 创建临时目录
    /// </summary>
    private string CreateTempDirectory()
    {
        var tempDir = Path.Combine(_tempBaseDir, Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        _tempDirsToCleanup.Add(tempDir);
        return tempDir;
    }

    /// <summary>
    /// 安全删除目录
    /// </summary>
    private void SafeDeleteDirectory(string directoryPath)
    {
        if (string.IsNullOrEmpty(directoryPath) || !Directory.Exists(directoryPath))
            return;

        try
        {
            // 先尝试删除所有文件
            foreach (var file in Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories))
            {
                try
                {
                    File.Delete(file);
                }
                catch
                {
                    // 忽略单个文件删除失败
                }
            }

            // 然后删除目录
            Directory.Delete(directoryPath, true);
            _tempDirsToCleanup.Remove(directoryPath);
        }
        catch
        {
            // 记录日志或忽略删除失败
            // 目录会在程序退出时由 Dispose 方法统一清理
        }
    }

    /// <summary>
    /// 转义命令行参数
    /// </summary>
    private string EscapeArgument(string arg)
    {
        if (string.IsNullOrEmpty(arg))
            return "\"\"";

        // 如果参数包含空格或引号，需要转义
        if (arg.Contains(' ') || arg.Contains('"') || arg.Contains('\''))
        {
            return $"\"{arg.Replace("\"", "\\\"")}\"";
        }

        return arg;
    }

    /// <summary>
    /// 转义路径（处理Windows路径问题）
    /// </summary>
    private string EscapePath(string path)
    {
        if (Path.DirectorySeparatorChar == '\\')
        {
            // Windows系统：将反斜杠转换为正斜杠，并转义特殊字符
            return path.Replace("\\", "/").Replace(":", "\\:");
        }

        return path;
    }

    /// <summary>
    /// 实现 IDisposable 接口，确保资源清理
    /// </summary>
    public void Dispose()
    {
        foreach (var dir in _tempDirsToCleanup.ToList())
        {
            SafeDeleteDirectory(dir);
        }

        // 尝试清理基目录（如果为空）
        try
        {
            if (Directory.Exists(_tempBaseDir) &&
                !Directory.EnumerateFileSystemEntries(_tempBaseDir).Any())
            {
                Directory.Delete(_tempBaseDir);
            }
        }
        catch
        {
            // 忽略清理失败
        }

        GC.SuppressFinalize(this);
    }

    ~DockerCodeExecutor()
    {
        Dispose();
    }
}