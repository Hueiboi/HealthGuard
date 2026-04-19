using HealthGuard.Data;
using HealthGuard.Models.Dto;
using HealthGuard.Models.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthGuard.Services
{
    public class PatientProfileService
    {
        private readonly HealthContext _context;

        // Tiêm HealthContext trực tiếp, dọn dẹp Repo và Mapper cũ
        public PatientProfileService(HealthContext context)
        {
            _context = context;
        }

        public async Task<PatientProfileDto> GetMyProfileAsync(string username)
        {
            // Sử dụng LINQ để tìm Patient dựa trên Username của bảng User liên kết
            var myProfile = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.User.Username == username);

            if (myProfile == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy hồ sơ bệnh của người dùng: {username}");
            }

            // Map thủ công sang DTO
            return new PatientProfileDto
            {
                Id = myProfile.Id,
                FullName = myProfile.FullName,
                Email = myProfile.User.Email,
                PhoneNumber = myProfile.User.PhoneNumber
            };
        }

        public async Task<PatientProfileDto> UpdateProfileAsync(PatientProfileDto request, string username)
        {
            var existingPatient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.User.Username == username);

            if (existingPatient == null) throw new KeyNotFoundException("Không tìm thấy hồ sơ");

            // Cập nhật thông tin
            existingPatient.FullName = request.FullName;
            existingPatient.User.PhoneNumber = request.PhoneNumber;

            // 👉 LƯU THÊM CÁC THÔNG TIN Y TẾ MỚI TỪ FORM
            existingPatient.Height = request.Height;
            existingPatient.Weight = request.Weight;
            existingPatient.MedicalHistory = request.MedicalHistory;
            // existingPatient.DateOfBirth = ... (Tuỳ DB của ông)

            await _context.SaveChangesAsync();
            return request;
        }
    }
}