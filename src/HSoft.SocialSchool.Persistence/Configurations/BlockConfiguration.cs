using HSoft.SocialSchool.Domain.Entities.Blocks;
using HSoft.SocialSchool.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HSoft.SocialSchool.Persistence.Configurations
{
    internal sealed class BlockConfiguration : IEntityTypeConfiguration<Block>
    {
        public void Configure(EntityTypeBuilder<Block> builder)
        {
            builder.ToTable("Blocks");

            builder.HasKey(block => block.Id);
            builder.Property(block => block.Id).ValueGeneratedNever();

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(block => block.BlockerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(block => block.BlockedId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(block => new { block.BlockerId, block.BlockedId })
                .IsUnique()
                .HasDatabaseName("IX_Blocks_BlockerId_BlockedId");
        }
    }
}
