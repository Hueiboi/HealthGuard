using HealthGuard.Data;
using HealthGuard.Models.Dto;
using HealthGuard.Models.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading.Tasks;

namespace HealthGuard.Services
{
    public class PatientProfileService
    {
        private readonly HealthContext _context;

        public PatientProfileService(HealthContext context)
        {
            _context = context;
        }

        public async Task<PatientProfileDto> GetMyProfileAsync(string username)
        {
            var myProfile = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.User.Username == username);

            if (myProfile == null)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (user == null) throw new Exception("Không tìm thấy tài khoản!");

                myProfile = new Patient
                {
                    User = user,
                    FullName = "Chưa cập nhật tên"
                };

                _context.Patients.Add(myProfile);
                await _context.SaveChangesAsync();
            }

            return new PatientProfileDto
            {
                Id = myProfile.Id,
                FullName = myProfile.FullName,
                Email = myProfile.User?.Email,
                PhoneNumber = myProfile.User?.PhoneNumber,

                DateOfBirth = myProfile.DateOfBirth,
                EmergencyContact = myProfile.EmergencyContact,
                MedicalHistory = myProfile.MedicalHistory,
                Height = myProfile.Height,
                Weight = myProfile.Weight,

                // 👉 ĐÃ FIX: THÊM 4 DÒNG NÀY ĐỂ API TRẢ ĐỦ DỮ LIỆU VỀ APP
                AvatarUrl = myProfile.AvatarUrl,
                Gender = myProfile.Gender,
                BloodType = myProfile.BloodType,
                Allergies = myProfile.Allergies
            };
        }

        public async Task UpdateProfileAsync(PatientProfileDto request, string username)
        {
            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.User.Username == username);

            if (patient == null) throw new Exception("Không tìm thấy hồ sơ!");

            patient.FullName = request.FullName;
            patient.DateOfBirth = request.DateOfBirth;
            patient.EmergencyContact = request.EmergencyContact;
            patient.MedicalHistory = request.MedicalHistory;
            patient.Height = request.Height;
            patient.Weight = request.Weight;
            patient.AvatarUrl = request.AvatarUrl;
            patient.Gender = request.Gender;
            patient.BloodType = request.BloodType;
            patient.Allergies = request.Allergies;

            if (!string.IsNullOrEmpty(request.AvatarUrl) && request.AvatarUrl.StartsWith("data:image"))
            {
                try
                {
                    var base64Data = request.AvatarUrl.Substring(request.AvatarUrl.IndexOf(",") + 1);
                    byte[] imageBytes = Convert.FromBase64String(base64Data);

                    string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    if (!Directory.Exists(uploadFolder))
                    {
                        Directory.CreateDirectory(uploadFolder);
                    }

                    string fileName = $"avatar_{username}_{DateTime.Now.Ticks}.jpg";
                    string filePath = Path.Combine(uploadFolder, fileName);

                    await File.WriteAllBytesAsync(filePath, imageBytes);

                    patient.AvatarUrl = $"/uploads/{fileName}";
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Lỗi lưu ảnh: " + ex.Message);
                }
            }

            _context.Patients.Update(patient);
            await _context.SaveChangesAsync();
        }
    }
}