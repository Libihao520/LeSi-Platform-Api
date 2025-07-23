using EFCoreMigrations;
using LeSi.Admin.WebApi;
using Microsoft.EntityFrameworkCore;
using WebApi.Config;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Register();

builder.Services.AddGrpcClient<AuthService.AuthServiceClient>(options =>
{
    options.Address = new Uri("http://localhost:5159");
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


app.MapControllers();

app.Run();