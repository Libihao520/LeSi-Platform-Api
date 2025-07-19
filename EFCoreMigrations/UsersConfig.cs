using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Model.Entities;

namespace EFCoreMigrations;

class UsersConfig : IEntityTypeConfiguration<Users>
{
    public void Configure(EntityTypeBuilder<Users> builder)
    {
        builder.ToTable("Users")
            .HasIndex(e => e.Name)
            .IsUnique()
            .HasDatabaseName("IX_Users_UniqueName");
    }
}