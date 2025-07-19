using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Model.Entities;

namespace EFCoreMigrations;

public class YoloConfig : IEntityTypeConfiguration<YoLoTbs>
{
    public void Configure(EntityTypeBuilder<YoLoTbs> builder)
    {
        builder.ToTable("YoLoTbs");
    }
}