using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthGuard.Models.Entity 
{
    public class User
    {
        [Key]
        public long Id { get; set; } // Sửa từ int thành long để khớp với UserResponseDto và CSDL lớn

        [Required]
        [MaxLength(50)]
        public string? Username { get; set; }

        [Required]
        [EmailAddress] // Thêm validate chuẩn Email
        [MaxLength(100)]
        public string Email { get; set; } // THIẾU: AuthService đang dùng trường này để đăng nhập và check trùng

        [Required]
        public string Password { get; set; } // Đổi PasswordHash -> Password cho khớp với code AuthService ban nãy

        [MaxLength(15)]
        public string PhoneNumber { get; set; } // Để không Required vì lúc Register mình chưa bắt nhập số điện thoại

        public bool IsActive { get; set; } // THIẾU: CustomUserDetailService cần trường này để check user bị khóa chưa

        public DateTime CreatedAt { get; set; } // THIẾU: Cần để lưu thời gian tạo tài khoản

        // ==========================================
        // KHÓA NGOẠI VÀ NAVIGATION PROPERTIES
        // ==========================================

        [ForeignKey("Role")]
        public long RoleId { get; set; } // Sửa thành long cho khớp với RoleDto.Id
        public virtual Role Role { get; set; }

        // Mối quan hệ 1-1 với bảng Patient (Hồ sơ bệnh nhân)
        // Vì bên AuthService lúc tạo User xong mình có tạo luôn Patient
        public virtual Patient Patient { get; set; }
    }
}