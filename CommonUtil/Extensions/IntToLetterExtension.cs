namespace CommonUtil.Extensions;

public static class IntToLetterExtension
{
    /// <summary>
    /// int的扩展方法，练题系统选项转换成ABCD
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string ToLetter(this int value)
    {
        if (value >= 0 && value <= 3)
        {
            return new char[] { 'A', 'B', 'C', 'D' }[value].ToString();
        }
        return "Unknown"; 
    }
}