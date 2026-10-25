using HSoft.SocialSchool.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HSoft.SocialSchool.Persistence.Configurations
{
    internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(user => user.Id);
            builder.Property(user => user.Id).ValueGeneratedNever();

            builder.Property(user => user.Email).IsRequired().HasMaxLength(320);

            // Nullable: la cuenta nace desde Keycloak sin nick y lo fija el usuario al completar el perfil.
            // El índice único sigue siendo válido porque MySQL admite varios nulos.
            builder.Property(user => user.Nickname).HasMaxLength(50);
            builder.Property(user => user.IdentityProvider).IsRequired().HasMaxLength(50);
            builder.Property(user => user.ExternalIdentityId).IsRequired().HasMaxLength(200);
            builder.Property(user => user.PreferredLanguage).HasMaxLength(10);

            builder.HasIndex(user => user.Nickname).IsUnique().HasDatabaseName("IX_Users_Nickname");

            // Clave de la búsqueda del perfil propio: se consulta por este campo en cada llamada y una
            // identidad de Keycloak solo puede tener una cuenta.
            builder.HasIndex(user => user.ExternalIdentityId).IsUnique().HasDatabaseName("IX_Users_ExternalIdentityId");

            // La relación se declara desde el lado principal para que EF use el backing field de la colección.
            builder.HasMany(user => user.NotificationPreferences)
                .WithOne()
                .HasForeignKey(preference => preference.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
