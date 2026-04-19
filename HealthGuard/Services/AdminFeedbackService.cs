using HealthGuard.Models.Dto;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HealthGuard.Data; // Nhớ check lại using này cho đúng chỗ chứa HealthContext của ông

namespace HealthGuard.Services
{
    public class AdminFeedbackService
    {
        private readonly HealthContext _context;

        // Chỉ tiêm DbContext, tạm biệt Repository và Mapper cồng kềnh của Java
        public AdminFeedbackService(HealthContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<FeedbackResponseDto>> GetAllFeedbacksAsync(int page, int size)
        {
            // LINQ xử lý phân trang và map DTO cực kỳ gọn gàng
            var feedbacks = await _context.Feedbacks
                .Include(f => f.User) // Join bảng User để lấy Username
                .OrderByDescending(f => f.CreatedAt)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(f => new FeedbackResponseDto
                {
                    Id = f.Id,
                    SessionId = f.SessionId, // Thay bằng DiagnosticSessionId nếu Entity của ông đặt tên thế
                    UserId = f.UserId,
                    Username = f.User.Username,
                    Comments = f.Comments,
                    CreatedAt = f.CreatedAt
                })
                .ToListAsync();

            return feedbacks;
        }

        public async Task<FeedbackResponseDto> GetFeedbackByIdAsync(long id)
        {
            // Include bảng User để lấy thông tin Username
            var feedback = await _context.Feedbacks
                .Include(f => f.User)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (feedback == null)
            {
                // Dùng Exception chuẩn của .NET thay cho Exception chung chung
                throw new KeyNotFoundException($"Không tìm thấy phản hồi với ID: {id}");
            }

            return new FeedbackResponseDto
            {
                Id = feedback.Id,
                SessionId = feedback.SessionId,
                UserId = feedback.UserId,
                Username = feedback.User.Username,
                Comments = feedback.Comments,
                CreatedAt = feedback.CreatedAt
            };
        }
    }
}