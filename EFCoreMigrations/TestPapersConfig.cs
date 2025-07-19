using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Model.Entities;

namespace EFCoreMigrations;


class TestPapersConfig:IEntityTypeConfiguration<TestPapers>
{
    public void Configure(EntityTypeBuilder<TestPapers> builder)
    {
        builder.ToTable("TestPapers");
    }
}