using DocuSense.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DocuSense.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Document> Documents { get; set; }
        public DbSet<DocumentField> DocumentFields { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Document configuration
            builder.Entity<Document>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.FileType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.BlobUrl).IsRequired().HasMaxLength(500);
                entity.Property(e => e.ContainerName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.BlobName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.ProjectName).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.DocumentCategory).HasMaxLength(50);
                entity.Property(e => e.ProcessingResult).HasMaxLength(1000);
                entity.Property(e => e.ErrorMessage).HasMaxLength(500);
                entity.Property(e => e.ProcessingType).HasMaxLength(50);

                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.UploadedAt);
                entity.HasIndex(e => e.ProjectName);
                entity.HasIndex(e => e.DocumentCategory);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Documents)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // DocumentField configuration
            builder.Entity<DocumentField>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DocumentId).IsRequired();
                entity.Property(e => e.FieldName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.FieldValue).HasMaxLength(500);
                entity.Property(e => e.FieldType).HasMaxLength(50);
                entity.Property(e => e.BoundingBox).HasMaxLength(100);
                entity.Property(e => e.ExtractedBy).HasMaxLength(100);
                entity.Property(e => e.VerifiedBy).HasMaxLength(450);
                entity.Property(e => e.Notes).HasMaxLength(500);

                entity.HasIndex(e => e.DocumentId);
                entity.HasIndex(e => e.FieldName);
                entity.HasIndex(e => e.IsVerified);

                entity.HasOne(e => e.Document)
                    .WithMany(d => d.ExtractedFields)
                    .HasForeignKey(e => e.DocumentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // AuditLog configuration
            builder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Timestamp).IsRequired();
                entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
                entity.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.EntityId).HasMaxLength(450);
                entity.Property(e => e.UserId).HasMaxLength(450);
                entity.Property(e => e.UserEmail).HasMaxLength(100);
                entity.Property(e => e.UserRole).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.IpAddress).HasMaxLength(50);
                entity.Property(e => e.UserAgent).HasMaxLength(500);
                entity.Property(e => e.Status).HasMaxLength(100);
                entity.Property(e => e.Details).HasMaxLength(1000);
                entity.Property(e => e.Severity).HasMaxLength(100);
                entity.Property(e => e.AnomalyReason).HasMaxLength(500);

                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.Action);
                entity.HasIndex(e => e.EntityType);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Severity);
                entity.HasIndex(e => e.IsAnomaly);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.AuditLogs)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Document)
                    .WithMany(d => d.AuditLogs)
                    .HasForeignKey(e => e.EntityId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ApplicationUser configuration
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(e => e.FirstName).HasMaxLength(100);
                entity.Property(e => e.LastName).HasMaxLength(100);
                entity.Property(e => e.Company).HasMaxLength(100);
                entity.Property(e => e.Department).HasMaxLength(100);
                entity.Property(e => e.Role).HasMaxLength(50);
                entity.Property(e => e.AzureAdObjectId).HasMaxLength(500);
                entity.Property(e => e.TenantId).HasMaxLength(100);

                entity.HasIndex(e => e.Email);
                entity.HasIndex(e => e.Role);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.AzureAdObjectId);
            });
        }
    }
} 