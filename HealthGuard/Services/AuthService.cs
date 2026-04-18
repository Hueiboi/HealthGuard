using BCrypt.Net;
using HealthGuard.Models.Dto;
using HealthGuard.Models.DTOs;
using HealthGuard.Models.Entities;
using HealthGuard.Models.Entity;
using HealthGuard.Models.model;
using HealthGuard.Repositories;
using HealthGuard.Security; // Thư mục chứa IJwtUtils giả định
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Threading.Tasks;

namespace HealthGuard.Services
{
    // UC01: Đăng nhập/Đăng ký
    public class AuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IJwtUtils _jwtUtils;

        // Tiêm CustomUserDetailService vào đây để dùng giống luồng Spring Boot
        private readonly CustomUserDetailService _customUserDetailService;

        public AuthService(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IPatientRepository patientRepository,
            IJwtUtils jwtUtils,
            CustomUserDetailService customUserDetailService)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _patientRepository = patientRepository;
            _jwtUtils = jwtUtils;
            _customUserDetailService = customUserDetailService;
        }

        public async Task<UserResponseDTO> RegisterAsync(RegisterRequestDTO request)
        {
            if (await _userRepository.ExistsByUsernameAsync(request.Username))
            {
                throw new Exception("Tên đăng nhập đã tồn tại!");
            }

            if (await _userRepository.ExistsByEmailAsync(request.Email))
            {
                throw new Exception("Email đã được sử dụng!");
            }

            var userRole = await _roleRepository.FindByRoleNameAsync("ROLE_USER");
            if (userRole == null)
            {
                throw new Exception("Lỗi hệ thống: Không tìm thấy quyền ROLE_USER");
            }

            var newUser = new User
            {
                Username = request.Username,
                Email = request.Email,
                Role = userRole,
                IsActive = true,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                CreatedAt = DateTime.UtcNow
            };

            var savedUser = await _userRepository.SaveAsync(newUser);

            var newPatient = new Patient
            {
                User = savedUser,
                FullName = request.Username
            };
            await _patientRepository.SaveAsync(newPatient);

            return new UserResponseDTO
            {
                Id = savedUser.Id,
                Username = savedUser.Username,
                Email = savedUser.Email,
                RoleName = savedUser.Role.RoleName,
                IsActive = savedUser.IsActive,
                CreatedAt = savedUser.CreatedAt
            };
        }

        public async Task<string> LoginAsync(LoginRequestDTO request)
        {
            // 1. Nhờ CustomUserDetailService kiểm tra xem User có tồn tại và đang active không
            // (Giống hệt cách Spring Security gọi loadUserByUsername)
            var user = await _customUserDetailService.LoadUserByUsernameAsync(request.EmailOrUsername);

            // 2. Kiểm tra mật khẩu bằng BCrypt
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                throw new Exception("Sai tài khoản hoặc mật khẩu!");
            }

            // 3. Sinh JWT Token
            return _jwtUtils.GenerateJwtToken(user);
        }
    }
}