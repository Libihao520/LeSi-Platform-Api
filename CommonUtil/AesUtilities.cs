using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace CommonUtil
{
    public class AesUtilities
    {
        private static byte[] _keyArray = Encoding.UTF8.GetBytes("C#ACXJAesCode@#!");
        private static byte[] _ivArray = Encoding.UTF8.GetBytes("ACXJV1024AESCODE");

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="content"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string Encrypt(string content)
        {
            try
            {
                var toEncryptArray = Encoding.UTF8.GetBytes(content);
                var aes = Aes.Create();
                aes.Key = _keyArray;
                aes.IV = _ivArray;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                var cTransform = aes.CreateEncryptor();
                var resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                return Convert.ToBase64String(resultArray);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return "???";
            }
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="content"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string Decrypt(string content)
        {
            try
            {
                var toEncryptArray = Convert.FromBase64String(content);
                var aes = Aes.Create();
                aes.Key = _keyArray;
                aes.IV = _ivArray;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var cTransform = aes.CreateDecryptor())
                {
                    var resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

                    return Encoding.UTF8.GetString(resultArray);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return "???";
            }
        }
    }
}