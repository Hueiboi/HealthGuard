using HealthGuard.Models.Entity; // Chuẩn folder số ít của ông
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace HealthGuard.Data
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(HealthContext context)
        {
            try
            {
                var userRole = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "ROLE_USER");
                if (userRole == null)
                {
                    userRole = new Role { RoleName = "ROLE_USER" };
                    context.Roles.Add(userRole);
                }

                var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "ROLE_ADMIN");
                if (adminRole == null)
                {
                    adminRole = new Role { RoleName = "ROLE_ADMIN" };
                    context.Roles.Add(adminRole);
                }

                await context.SaveChangesAsync();
                Console.WriteLine("Đã kiểm tra và khởi tạo danh mục Roles thành công!");

                var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
                if (adminUser == null)
                {
                    adminUser = new User
                    {
                        Username = "admin",
                        Email = "admin@diagnose.com",
                        Password = BCrypt.Net.BCrypt.HashPassword("123456"),
                        Role = adminRole,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        
                        PhoneNumber = "0123456789"
                    };
                    context.Users.Add(adminUser);
                    await context.SaveChangesAsync();
                    Console.WriteLine("Đã khởi tạo tài khoản Admin mặc định!");
                }
                if (!await context.Symptoms.AnyAsync())
                {
                    context.Symptoms.AddRange(
                        new Symptom { Id = 101, SymptomName = "Fever" },
                        new Symptom { Id = 102, SymptomName = "Headache" },
                        new Symptom { Id = 103, SymptomName = "Nausea" },
                        new Symptom { Id = 104, SymptomName = "Fatigue" },
                        new Symptom { Id = 105, SymptomName = "Body Aches" },
                        new Symptom { Id = 106, SymptomName = "Shortness of Breath" }
                    );
                    await context.SaveChangesAsync();
                    Console.WriteLine("Đã bơm dữ liệu Triệu chứng vào Database!");
                }
            }
            catch (DbUpdateException ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n=================================================");
                Console.WriteLine($"[LỖI CƠ SỞ DỮ LIỆU KHI SEED]: {innerMessage}");
                Console.WriteLine("=================================================\n");
                Console.ResetColor();

                throw; 
            }
        }
    }
}