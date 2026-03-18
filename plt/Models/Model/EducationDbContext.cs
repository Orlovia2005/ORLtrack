using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.Security.Claims;

namespace plt.Models.Model
{
    public partial class EducationDbContext : DbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public EducationDbContext(
            DbContextOptions<EducationDbContext> options,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Student> Students { get; set; }
        public virtual DbSet<StudentLesson> StudentLessons { get; set; }
        public virtual DbSet<StudentPayment> StudentPayments { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection")
                    ?? "Host=localhost;Port=5432;Database=Platforma;Username=postgres;Password=Ignat2005;";
                optionsBuilder.UseNpgsql(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("users_pkey");

                entity.HasIndex(e => e.Email)
                    .IsUnique()
                    .HasDatabaseName("ix_users_email");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("name");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("email");

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnName("password");
            });

            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("students_pkey");
                entity.ToTable("Students");

                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).HasMaxLength(100);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.Balance).HasPrecision(18, 2);
                entity.Property(e => e.LessonRate).HasPrecision(18, 2);
                entity.Property(e => e.TotalPaidIn).HasPrecision(18, 2);
                entity.Property(e => e.TotalCharged).HasPrecision(18, 2);

                entity.HasOne(e => e.Teacher)
                    .WithMany(e => e.Students)
                    .HasForeignKey(e => e.TeacherId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<StudentLesson>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("student_lessons_pkey");
                entity.ToTable("StudentLessons");

                entity.Property(e => e.ChargedAmount).HasPrecision(18, 2);
                entity.Property(e => e.Comment).HasMaxLength(250);

                entity.HasOne(e => e.Student)
                    .WithMany(e => e.Lessons)
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Teacher)
                    .WithMany(e => e.StudentLessons)
                    .HasForeignKey(e => e.TeacherId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<StudentPayment>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("student_payments_pkey");
                entity.ToTable("StudentPayments");

                entity.Property(e => e.Amount).HasPrecision(18, 2);
                entity.Property(e => e.Comment).HasMaxLength(250);

                entity.HasOne(e => e.Student)
                    .WithMany(e => e.Payments)
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Teacher)
                    .WithMany(e => e.StudentPayments)
                    .HasForeignKey(e => e.TeacherId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var auditEntries = OnBeforeSaveChanges();
            var result = base.SaveChangesAsync(cancellationToken);
            OnAfterSaveChanges(auditEntries);
            return result;
        }

        public override int SaveChanges()
        {
            var auditEntries = OnBeforeSaveChanges();
            var result = base.SaveChanges();
            OnAfterSaveChanges(auditEntries);
            return result;
        }

        private List<AuditEntry> OnBeforeSaveChanges()
        {
            ChangeTracker.DetectChanges();
            var auditEntries = new List<AuditEntry>();
            var userId = GetCurrentUserId();

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Added || entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
                {
                    var auditEntry = new AuditEntry(entry)
                    {
                        TableName = entry.Metadata.GetTableName() ?? "Unknown",
                        Action = entry.State.ToString(),
                        UserId = userId,
                    };

                    foreach (var property in entry.Properties)
                    {
                        var propertyName = property.Metadata.Name;
                        auditEntry.NewValues[propertyName] = property.CurrentValue ?? "NULL";
                    }

                    auditEntries.Add(auditEntry);
                }
            }

            return auditEntries;
        }

        private void OnAfterSaveChanges(List<AuditEntry> auditEntries)
        {
            if (!auditEntries.Any())
            {
                return;
            }

            var ip = _httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown";

            foreach (var auditEntry in auditEntries)
            {
                Log.Information(
                    "Изменения в таблице {TableName}, действие {Action}, пользователь {UserId}, IP {IP}, новые значения: {Value}",
                    auditEntry.TableName,
                    auditEntry.Action,
                    auditEntry.UserId,
                    ip,
                    auditEntry.NewValues);
            }
        }

        private string GetCurrentUserId()
        {
            return _httpContextAccessor?.HttpContext?.User?.FindFirstValue("Id") ?? "Unauthorized";
        }
    }

    public class AuditEntry
    {
        public string TableName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public Dictionary<string, object> NewValues { get; set; } = new();

        public AuditEntry(EntityEntry entry)
        {
        }
    }
}
