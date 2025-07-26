using System.Security;
using System.Security.Cryptography;
using System.Text.Json;
using CommonUtil.RedisUtil;
using Model.UtilData;

namespace WebApi.Config;

public class KeyResolverService
{
    public static RSA GetPublicKeyFromDynamicSource(string token)
    {
        var publicKey = CacheManager.Get<string>(token);
        if (string.IsNullOrEmpty(publicKey))
        {
            throw new SecurityException("公钥已过期或无效，请重新获取授权");
        }

        try
        {
            // 成功获取公钥后，刷新缓存时间，续费半小时
            CacheManager.Set(token, publicKey, TimeSpan.FromMinutes(30));
            var deserialize = JsonSerializer.Deserialize<string>(publicKey);
            var rsa = RSA.Create();
            rsa.ImportFromPem(deserialize);
            return rsa;
        }
        catch (JsonException ex)
        {
            throw new SecurityException("公钥格式无效", ex);
        }
        catch (CryptographicException ex)
        {
            throw new SecurityException("公钥解析失败", ex);
        }
    }
}