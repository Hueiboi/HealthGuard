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

            // 1. RÀNG BUỘC DUY NHẤT (UNIQUE)
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username).IsUnique();
            modelBuilder.Entity<User>()
                .HasIndex(u => u.PhoneNumber).IsUnique();
            modelBuilder.Entity<Disease>()
                .HasIndex(d => d.DiseaseCode).IsUnique(); // Mã bệnh phải là duy nhất

            // 2. KHÓA PHỨC HỢP (COMPOSITE KEYS) - BẮT BUỘC PHẢI CÓ
            // Bảng liên kết Bệnh - Triệu chứng
            modelBuilder.Entity<DiseaseSymptom>()
                .HasKey(ds => new { ds.DiseaseId, ds.SymptomId });

            // Bảng liên kết Phiên khám - Triệu chứng
            modelBuilder.Entity<SessionSymptom>()
                .HasKey(ss => new { ss.SessionId, ss.SymptomId });

            // 3. QUAN HỆ ĐẶC BIỆT (1-1)
            // Một User (nếu là USER) chỉ có 1 Hồ sơ Patient
            modelBuilder.Entity<Patient>()
                .HasOne(p => p.User)
                .WithOne()
                .HasForeignKey<Patient>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa User thì bay luôn Hồ sơ Patient

            // 4. DATA SEEDING (Dữ liệu mồi)
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, RoleName = "ROLE_ADMIN" },
                new Role { Id = 2, RoleName = "ROLE_USER" }
            );
        }
    }
}