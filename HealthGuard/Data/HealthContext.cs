using HealthGuard.Models.Entity; 
using Microsoft.EntityFrameworkCore;

namespace HealthGuard.Data
{
    public class HealthContext : DbContext
    {
        public HealthContext(DbContextOptions<HealthContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Symptom> Symptoms { get; set; }
        public DbSet<Disease> Diseases { get; set; }
        public DbSet<DiseaseSymptom> DiseaseSymptoms { get; set; }
        public DbSet<DiagnosticSession> DiagnosticSessions { get; set; }
        public DbSet<SessionSymptom> SessionSymptoms { get; set; }
        public DbSet<DiagnosisResult> DiagnosisResults { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username).IsUnique();
            modelBuilder.Entity<User>().HasIndex(u => u.PhoneNumber).IsUnique(false);
            modelBuilder.Entity<Disease>()
                .HasIndex(d => d.DiseaseCode).IsUnique(); 
            modelBuilder.Entity<DiseaseSymptom>()
                .HasKey(ds => new { ds.DiseaseId, ds.SymptomId });

            modelBuilder.Entity<SessionSymptom>()
                .HasIndex(ss => new { ss.DiagnosticSessionId, ss.SymptomId }).IsUnique();

            modelBuilder.Entity<Patient>()
                .HasOne(p => p.User)
                .WithOne(u => u.Patient) // <-- TRUYỀN THÊM CÁI NÀY VÀO LÀ EF HẾT BỊ LÚ
                .HasForeignKey<Patient>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Bồi thêm 1 đòn chí mạng để chắc chắn UserId1 không bao giờ quay lại
            modelBuilder.Entity<Patient>().Ignore("UserId1");

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, RoleName = "ROLE_ADMIN" },
                new Role { Id = 2, RoleName = "ROLE_USER" }
            );
        }
    }
}