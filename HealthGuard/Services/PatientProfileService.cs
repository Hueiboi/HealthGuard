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
            // Tìm Patient hiện tại cùng thông tin User đi kèm
            var existingPatient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.User.Username == username);

            if (existingPatient == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy hồ sơ bệnh của người dùng: {username}");
            }

            // Cập nhật thông tin trực tiếp (Thay cho logic của Mapper cũ)
            existingPatient.FullName = request.FullName;
            existingPatient.User.PhoneNumber = request.PhoneNumber;
            // Lưu ý: Thường Email sẽ không cho phép cập nhật tại đây để đảm bảo tính duy nhất

            // EF Core tự động theo dõi thay đổi và thực hiện lệnh UPDATE khi SaveChanges
            await _context.SaveChangesAsync();

            return new PatientProfileDto
            {
                Id = existingPatient.Id,
                FullName = existingPatient.FullName,
                Email = existingPatient.User.Email,
                PhoneNumber = existingPatient.User.PhoneNumber
            };
        }
    }
}