using Microsoft.EntityFrameworkCore;
using Model.Entities;

namespace EFCoreMigrations;

public class MyDbContext : DbContext
{
    /// <summary>
    /// 用户表
    /// </summary>
    public DbSet<Users> Users { get; set; }

    /// <summary>
    /// yolo识别记录表
    /// </summary>
    public DbSet<YoLoTbs> YoLoTbs { get; set; }

    /// <summary>
    /// 图片base64
    /// </summary>
    public DbSet<Photos> Photos { get; set; }

    /// <summary>
    /// 成绩中心
    /// </summary>
    public DbSet<ReportCard> ReportCards { get; set; }

    /// <summary>
    /// 模型管理
    /// </summary>
    public DbSet<AiModels> AiModels { get; set; }

    /// <summary>
    /// 题目
    /// </summary>
    public DbSet<TestPapers> TestPapers { get; set; }

    /// <summary>
    ///题库
    /// </summary>
    public DbSet<TestPapersManage> TestPapersManages { get; set; }

    public MyDbContext()
    {
    }

    // 注入方式配置
    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
    {
    }

    //数据库迁移
    // cd EFCoreMigrations
    //dotnet ef migrations add InitialCreate
    //dotnet ef database update
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        //数据库连接字符串
        optionsBuilder.UseMySql("server=mysql;port=3306;database=aitest;user=root;password=1qazZAQ!hhh333",
            new MySqlServerVersion(new Version(8, 0, 33)));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 自动加载所有配置类
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }
}