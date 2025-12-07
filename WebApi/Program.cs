using EFCoreMigrations;
using LeSi.Admin.WebApi;
using Microsoft.EntityFrameworkCore;
using WebApi.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using NLog.Web;
using Service.SignalR;

var builder = WebApplication.CreateBuilder(args);

// 添加NLog服务
builder.Logging.ClearProviders();
builder.Host.UseNLog();

builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddJsonFile(
        $"appsettings.{(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true" ? "Docker" : "Development")}.json",
        optional: true);
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
            
    // 添加 JWT Bearer 认证配置
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Description = "请输入token格式为Bearer xxxxxx(中间必须有空格)",
        Name = "Authorization",
        In = ParameterLocation.Header,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme()
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });

    options.SwaggerDoc("v1", new OpenApiInfo { Title = $"LeSi Admin API", Version = "v1" });
});
builder.Register();

var grpcSettings = builder.Configuration.GetSection("GrpcSettings");
builder.Services.AddGrpcClient<AuthService.AuthServiceClient>(options =>
{
    options.Address = new Uri(grpcSettings["AuthServiceUrl"]);
});

//注入MyDbcontext
builder.Services.AddDbContext<MyDbContext>(p =>
{
    // p.UseSqlServer(builder.Configuration.GetConnectionString("SQL"));
    p.UseMySql(builder.Configuration.GetConnectionString("MySQL"), new MySqlServerVersion(new Version(8, 0, 33)));
});
// 注册 IHttpContextAccessor  
builder.Services.AddHttpContextAccessor();

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
//HTTPS 重定向
// app.UseHttpsRedirection();

//使用跨域策略
app.UseCors("CorsPolicy");

#region 鉴权授权

//通过 ASP.NET Core 中配置的授权认证，读取客户端中的身份标识(Cookie,Token等)并解析出来，存储到 context.User 中
app.UseAuthentication();
//判断当前访问 Endpoint (Controller或Action)是否使用了 [Authorize]以及配置角色或策略，然后校验 Cookie 或 Token 是否有效
app.UseAuthorization();

#endregion

app.MapHub<RecognitionHub>("/api/recognitionHub");

app.MapControllers();

app.Lifetime.ApplicationStopped.Register(() => NLog.LogManager.Shutdown());

app.Run();