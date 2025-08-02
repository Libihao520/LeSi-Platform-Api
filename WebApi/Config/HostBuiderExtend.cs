using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using CommonUtil;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Model.Options;
using Model.Other;
using RabbitMQ.Client;

namespace WebApi.Config;

public static class HostBuiderExtend
{
    public static void Register(this WebApplicationBuilder builder)
    {
        builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
        builder.Host.ConfigureContainer<ContainerBuilder>(builder =>
        {
            //注册接口和实现层
            builder.RegisterModule(new AutofacModuleRegister());
        });
        //Automapper映射
        builder.Services.AddAutoMapper(typeof(AutoMapperConfigs));
        //读取appsettings的JWTTokenOptions，注册JWT
        builder.Services.Configure<JWTTokenOptions>(builder.Configuration.GetSection("JWTTokenOptions"));
        //注册邮箱配置
        builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("EmailOptions"));
        //AI模块配置
        builder.Services.Configure<List<AiGcService>>(builder.Configuration.GetSection("AiGcOptions"));

        // 添加 RabbitMQ 连接服务
        builder.Services.AddSingleton<IConnection>(sp =>
        {
            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                Port = 5672,
                UserName = "admin",
                Password = "password"
            };
            return factory.CreateConnection();
        });

        #region JWT校验

        //第二步，增加鉴权逻辑
        JWTTokenOptions tokenOptions = new JWTTokenOptions();
        builder.Configuration.Bind("JWTTokenOptions", tokenOptions);
        if (string.IsNullOrEmpty(tokenOptions.SecurityKey))
        {
            throw new InvalidOperationException("JWT SecurityKey is not configured.");
        }

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme) //Scheme
            .AddJwtBearer(options => //这里是配置的鉴权的逻辑
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    //JWT有一些默认的属性，就是给鉴权时就可以筛选了
                    ValidateIssuer = true, //是否验证Issuer
                    ValidateAudience = true, //是否验证Audience
                    ValidateLifetime = true, //是否验证失效时间
                    ValidateIssuerSigningKey = true, //是否验证SecurityKey
                    ValidAudience = tokenOptions.Audience, //
                    ValidIssuer = tokenOptions.Issuer, //Issuer，这两项和前面签发jwt的设置一致
                    IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
                    {
                        var publicKey = KeyResolverService.GetPublicKeyFromDynamicSource(token);
                        return new[] { new RsaSecurityKey(publicKey) };
                    }
                };
            });

        #endregion

        //添加跨域策略
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy",
                opt => opt
                    .WithOrigins("http://127.0.0.1:8080") // 前端地址
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials() // 允许带凭证（如 SignalR、JWT）
                    .WithExposedHeaders("X-Pagination"));
        });
    }
}