using HealthGuard.Models.Entity; // Đảm bảo đúng folder Entity số ít của ông
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

            // 2. KHÓA PHỨC HỢP (COMPOSITE KEYS) VÀ RÀNG BUỘC

            // Bảng liên kết Bệnh - Triệu chứng (Vì bảng này thường không có cột Id riêng nên dùng HasKey là chuẩn)
            modelBuilder.Entity<DiseaseSymptom>()
                .HasKey(ds => new { ds.DiseaseId, ds.SymptomId });

            // ĐÃ FIX: Bảng liên kết Phiên khám - Triệu chứng
            // Đổi SessionId thành DiagnosticSessionId cho khớp Entity. 
            // Đổi HasKey thành HasIndex().IsUnique() vì bảng này đã có cột Id làm khóa chính.
            modelBuilder.Entity<SessionSymptom>()
                .HasIndex(ss => new { ss.DiagnosticSessionId, ss.SymptomId }).IsUnique();

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