using FCG02.UsersAPI.Domain.Entities;
using FCG02.UsersAPI.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FCG02.UsersAPI.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(u => u.Name)
            .HasColumnName("name")
            .HasMaxLength(150)
            .IsRequired();

        // mapeamento da propriedade Email para a coluna "email" na tabela "users"
        builder.Property(u => u.Email)
            .HasColumnName("email")
            .HasConversion(
                v => v.ToString(), // convert Email to string for storage
                v => new Email(v)  // convert string back to Email object when retrieving from the database
            )
            .HasMaxLength(256)
            .IsRequired();

        // garante unicidade do email no banco de dados
        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("ix_users_email");

        builder.Property(u => u.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(u => u.Role)
            .HasColumnName("role")
            .HasConversion<string>() // converte o enum para string para armazenamento
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(u => u.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired(false);
    }
}