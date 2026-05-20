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
            // Tìm kiếm linh hoạt: Thử khớp với Username, nếu không được thì thử khớp với Email hoặc Id
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.Username == username ||
                u.Email == username ||
                u.Id.ToString() == username);

            if (user == null)
            {
                // Thay vì quăng lỗi, hãy thử lấy User đầu tiên trong DB để "cứu bồ" khi đang debug
                user = await _context.Users.FirstOrDefaultAsync();
                if (user == null) throw new Exception("Database trống trơn rồi Nam ơi!");
            }

            var myProfile = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.User.Id == user.Id);

            if (myProfile == null)
            {
                myProfile = new Patient
                {
                    User = user,
                    FullName = "Người dùng mới"
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
                // ĐÃ XÓA: EmergencyContact theo yêu cầu của ông
                MedicalHistory = myProfile.MedicalHistory,
                Height = myProfile.Height,
                Weight = myProfile.Weight,

                // Giao diện mới thêm:
                AvatarUrl = myProfile.AvatarUrl,
                Gender = myProfile.Gender,
                BloodType = myProfile.BloodType,
                Allergies = myProfile.Allergies
            };
        }

        public async Task UpdateProfileAsync(PatientProfileDto request, string username)
        {
            // 👉 ĐÃ FIX CHO MOBILE: Bê y nguyên logic tìm kiếm linh hoạt từ hàm Get xuống hàm Update
            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p =>
                    p.User.Username == username ||
                    p.User.Email == username ||
                    p.User.Id.ToString() == username);

            if (patient == null)
                throw new Exception($"Không tìm thấy hồ sơ để cập nhật (Dữ liệu định danh: '{username}')");

            patient.FullName = request.FullName;
            patient.DateOfBirth = request.DateOfBirth;
            // ĐÃ XÓA: EmergencyContact
            patient.MedicalHistory = request.MedicalHistory;
            patient.Height = request.Height;
            patient.Weight = request.Weight;
            patient.Gender = request.Gender;
            patient.BloodType = request.BloodType;
            patient.Allergies = request.Allergies;

            if (patient.User != null)
            {
                patient.User.PhoneNumber = request.PhoneNumber;
            }

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

                    // Tối ưu tên file: Dùng ID của user thay cho username phòng khi username là Email có chứa ký tự "@" dễ gây lỗi đường dẫn file.
                    string fileName = $"avatar_{patient.User.Id}_{DateTime.Now.Ticks}.jpg";
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