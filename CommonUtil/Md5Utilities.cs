using System.Security.Cryptography;
using System.Text;

namespace CommonUtil;

/// <summary>
/// MD5 加密工具类
/// </summary>
public static class Md5Utilities
{
    /// <summary>
    /// MD5 加密
    /// </summary>
    /// <param name="fileStream"></param>
    /// <returns></returns>
    public static string GetMd5Hash(Stream fileStream)
    {
        var cryptBytes = MD5.HashData(fileStream);
        StringBuilder sb = new StringBuilder();
        foreach (var t in cryptBytes)
        {
            sb.Append(t.ToString("x2"));
        }

        return sb.ToString();
    }

    /// <summary>
    /// MD5 加密
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string GetMd5Hash(string input)
    {
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);

        byte[] hashBytes = MD5.HashData(inputBytes);

        // 将哈希值转换为十六进制字符串  
        StringBuilder sb = new();
        foreach (var t in hashBytes)
        {
            sb.Append(t.ToString("x2"));
        }

        // 返回最终的哈希字符串  
        return sb.ToString().ToLower();
    }
}