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
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace WebApi.Config;

public static class HostBuiderExtend
{
    public static void Register(this WebApplicationBuilder builder)
    {
        // 配置详细日志
        builder.Logging.AddFilter("Microsoft", LogLevel.Debug);
        builder.Logging.AddFilter("System", LogLevel.Debug);
        builder.Logging.AddFilter("Microsoft.AspNetCore.Authentication", LogLevel.Debug);
        builder.Logging.AddFilter("Microsoft.AspNetCore.Cors", LogLevel.Debug);
        builder.Logging.AddConsole();

        builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
        builder.Host.ConfigureContainer<ContainerBuilder>(builder =>
        {
            builder.RegisterModule(new AutofacModuleRegister());
        });

        builder.Services.AddAutoMapper(typeof(AutoMapperConfigs));
        builder.Services.Configure<JWTTokenOptions>(builder.Configuration.GetSection("JWTTokenOptions"));
        builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("EmailOptions"));
        builder.Services.Configure<List<AiGcService>>(builder.Configuration.GetSection("AiGcOptions"));

        // 添加 RabbitMQ 连接服务
        builder.Services.AddSingleton<IConnection>(sp =>
        {
            // 从配置中读取 RabbitMQ 设置
            var config = sp.GetRequiredService<IConfiguration>();
            var rabbitConfig = config.GetSection("RabbitMQ");
            var factory = new ConnectionFactory()
            {
                HostName = rabbitConfig["HostName"] ?? "localhost",
                Port = rabbitConfig.GetValue<int>("Port", 5672),
                UserName = rabbitConfig["UserName"] ?? "admin",
                Password = rabbitConfig["Password"] ?? "password",
                DispatchConsumersAsync = true,
                AutomaticRecoveryEnabled = true, // 启用自动恢复
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10) // 重试间隔
            };
            var maxRetryCount = 3;
            for (int retry = 1; retry <= maxRetryCount; retry++)
            {
                try
                {
                    return factory.CreateConnection();
                }
                catch (Exception ex) when (retry < maxRetryCount)
                {
                    var logger = sp.GetRequiredService<ILogger<Program>>();
                    logger.LogWarning(ex, "RabbitMQ连接失败，正在进行第 {RetryCount}/{MaxRetryCount} 次重试...", retry,
                        maxRetryCount);
                    Thread.Sleep(1000 * retry); // 指数退避
                }
            }

            // 最后一次尝试（如果失败会抛出异常）
            return factory.CreateConnection();
        });

        #region JWT校验

        JWTTokenOptions tokenOptions = new JWTTokenOptions();
        builder.Configuration.Bind("JWTTokenOptions", tokenOptions);
        if (string.IsNullOrEmpty(tokenOptions.SecurityKey))
        {
            throw new InvalidOperationException("JWT SecurityKey is not configured.");
        }

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidAudience = tokenOptions.Audience,
                    ValidIssuer = tokenOptions.Issuer,
                    IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
                    {
                        var publicKey = KeyResolverService.GetPublicKeyFromDynamicSource(token);
                        return new[] { new RsaSecurityKey(publicKey) };
                    }
                };

                // 添加JWT事件处理以记录详细日志
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Debug.WriteLine($"JWT Authentication Failed: {context.Exception}");
                        Console.WriteLine($"JWT Authentication Failed: {context.Exception}");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        Debug.WriteLine($"JWT Token Validated: {context.SecurityToken}");
                        Console.WriteLine($"JWT Token Validated: {context.SecurityToken}");
                        return Task.CompletedTask;
                    },
                    OnMessageReceived = context =>
                    {
                        Debug.WriteLine($"JWT Message Received: {context.Token}");
                        Console.WriteLine($"JWT Message Received: {context.Token}");
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        Debug.WriteLine($"JWT Challenge: {context.Error}, {context.ErrorDescription}");
                        Console.WriteLine($"JWT Challenge: {context.Error}, {context.ErrorDescription}");
                        return Task.CompletedTask;
                    }
                };
            });

        #endregion

        //添加跨域策略
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy",
                opt => opt
                    .WithOrigins("http://127.0.0.1:8080", "http://localhost:8080") // 添加多个可能的本地地址
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .WithExposedHeaders("X-Pagination"));

            // 添加一个日志记录中间件来跟踪CORS活动
            options.AddPolicy("DebugCorsPolicy", policy =>
            {
                policy.SetIsOriginAllowed(origin =>
                    {
                        Console.WriteLine($"CORS Origin Check: {origin}");
                        Debug.WriteLine($"CORS Origin Check: {origin}");
                        return true;
                    })
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        // 添加请求/响应日志中间件
        builder.Services.AddTransient<RequestResponseLoggingMiddleware>();
    }
}

// 添加一个中间件来记录所有请求和响应
public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        // 记录请求信息
        _logger.LogInformation($"Request: {context.Request.Method} {context.Request.Path}");
        _logger.LogInformation(
            $"Headers: {string.Join(", ", context.Request.Headers.Select(h => $"{h.Key}: {h.Value}"))}");

        if (context.Request.QueryString.HasValue)
        {
            _logger.LogInformation($"QueryString: {context.Request.QueryString}");
        }

        // 复制原始响应流以便我们可以读取它
        var originalBodyStream = context.Response.Body;
        using (var responseBody = new MemoryStream())
        {
            context.Response.Body = responseBody;

            await _next(context);

            // 记录响应信息
            _logger.LogInformation($"Response Status: {context.Response.StatusCode}");
            responseBody.Seek(0, SeekOrigin.Begin);
            var responseText = await new StreamReader(responseBody).ReadToEndAsync();
            _logger.LogInformation($"Response Body: {responseText}");
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
    }
}