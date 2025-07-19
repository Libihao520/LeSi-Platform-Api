using System.Text;

namespace CommonUtil.RandomIdUtil;

/// <summary>
/// 生成随机验证码（QQ邮箱注册）
/// </summary>
public abstract class RandomIdGenerator
{
    private static Random _random = new Random();
    private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    public static string GenerateRandomId(int length)
    {
        if (length < 1 || length > 10)
        {
            throw new ArgumentOutOfRangeException(nameof(length), "Length must be between 1 and 10.");
        }

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < length; i++)
        {
            int index = _random.Next(Chars.Length);
            char c = Chars[index];
            sb.Append(c);
        }

        return sb.ToString();
    }
}