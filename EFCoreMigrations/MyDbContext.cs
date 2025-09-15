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
    // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    // {
    //     base.OnConfiguring(optionsBuilder);
    //     //数据库连接字符串
    //     optionsBuilder.UseMySql("server=127.0.0.1;port=3306;database=aitest;user=root;password=1qazZAQ!hhh333",
    //         new MySqlServerVersion(new Version(8, 0, 33)));
    // }

    //CodeExam
    public DbSet<Question> Questions { get; set; }
    public DbSet<ExamRecord> ExamRecords { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 自动加载所有配置类
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
        //CodeExam
        base.OnModelCreating(modelBuilder);

        // 配置Question实体
        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(q => q.Id);
            entity.Property(q => q.Title).IsRequired().HasMaxLength(255);
            entity.Property(q => q.Description).IsRequired();
            entity.Property(q => q.ExampleInput).IsRequired();
            entity.Property(q => q.ExampleOutput).IsRequired();
        });

        // 配置ExamRecord实体
        modelBuilder.Entity<ExamRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.StudentCode).IsRequired();
            entity.HasOne(e => e.Question)
                  .WithMany()
                  .HasForeignKey(e => e.QuestionId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User) // 添加用户关系
              .WithMany(u => u.ExamRecords)
              .HasForeignKey(e => e.UserId)
              .OnDelete(DeleteBehavior.Cascade);
        });
    }
}