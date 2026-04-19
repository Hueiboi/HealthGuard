using HealthGuard.Data;
using HealthGuard.Models.Dto;
using HealthGuard.Models.Entity;
using Microsoft.EntityFrameworkCore;
using System;
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

        // ==========================================
        // 1. HÀM LẤY DỮ LIỆU TỪ DB LÊN GIAO DIỆN
        // ==========================================
        public async Task<PatientProfileDto> GetMyProfileAsync(string username)
        {
            var myProfile = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.User.Username == username);

            // NẾU CHƯA CÓ HỒ SƠ -> TỰ TẠO MỚI (Logic cũ tui giữ nguyên cho ông)
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

            // 👉 LỖI LÀ Ở ĐÂY LÚC TRƯỚC: Phải map ĐỦ TẤT CẢ các cột mới thêm vào DTO
            return new PatientProfileDto
            {
                Id = myProfile.Id,
                FullName = myProfile.FullName,
                Email = myProfile.User?.Email,
                PhoneNumber = myProfile.User?.PhoneNumber,

                // Mấy dòng này lúc trước bị thiếu nè:
                DateOfBirth = myProfile.DateOfBirth,
                EmergencyContact = myProfile.EmergencyContact,
                MedicalHistory = myProfile.MedicalHistory,
                Height = myProfile.Height,
                Weight = myProfile.Weight
            };
        }

        // ==========================================
        // 2. HÀM NHẬN DỮ LIỆU TỪ GIAO DIỆN LƯU XUỐNG DB
        // ==========================================
        public async Task UpdateProfileAsync(PatientProfileDto request, string username)
        {
            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.User.Username == username);

            if (patient == null) throw new Exception("Không tìm thấy hồ sơ!");

            // 👉 GÁN TẤT CẢ DỮ LIỆU (Bỏ luôn mấy cái điều kiện if>0 cho nó lưu sảng khoái)
            patient.FullName = request.FullName;
            patient.DateOfBirth = request.DateOfBirth;
            patient.EmergencyContact = request.EmergencyContact;
            patient.MedicalHistory = request.MedicalHistory;
            patient.Height = request.Height;
            patient.Weight = request.Weight;

            // Gán số điện thoại (Bảng User)
            if (patient.User != null && !string.IsNullOrEmpty(request.PhoneNumber))
            {
                patient.User.PhoneNumber = request.PhoneNumber;
            }

            // Cập nhật và lưu
            _context.Patients.Update(patient);
            await _context.SaveChangesAsync();
        }
    }
}