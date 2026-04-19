using HealthGuard.Data;
using HealthGuard.Models.Dto;
using HealthGuard.Models.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic; // Bổ sung để dùng KeyNotFoundException
using System.Threading.Tasks;

namespace HealthGuard.Services
{
    public class PatientFeedbackService
    {
        private readonly HealthContext _context;

        public PatientFeedbackService(HealthContext context)
        {
            _context = context;
        }

        public async Task<List<FeedbackResponseDto>> GetUserFeedbacksAsync(string username)
        {
            return await _context.Feedbacks
                .Include(f => f.DiagnosticSession)
                .Where(f => f.User.Username == username)
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => new FeedbackResponseDto
                {
                    Id = f.Id,
                    Comments = f.Comments,
                    CreatedAt = f.CreatedAt,
                    SessionId = f.SessionId
                })
                .ToListAsync();
        }

        public async Task<FeedbackResponseDto> SubmitFeedbackAsync(string username, FeedbackRequestDto request)
        {
            // 1. Tìm user
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                throw new UnauthorizedAccessException("Lỗi xác thực người dùng!");
            }

            // 2. Tạo mới Feedback
            var feedback = new Feedback
            {
                // ĐÃ FIX: Ép kiểu (int) đề phòng UserId trong Entity của ông đang là int
                UserId = (int)user.Id,
                User = user,
                Comments = request.Comments,
                CreatedAt = DateTime.UtcNow
            };

            // 3. Xử lý SessionId
            if (request.SessionId.HasValue)
            {
                // ĐÃ FIX: Lỗi không tìm thấy s.UserId -> Truy cập gián tiếp qua s.User.Id
                var session = await _context.DiagnosticSessions
                    .FirstOrDefaultAsync(s => s.Id == request.SessionId.Value && s.User.Id == user.Id);

                if (session == null)
                {
                    throw new KeyNotFoundException("Phiên khám không tồn tại hoặc không thuộc về bạn!");
                }

                // ĐÃ FIX: Ép kiểu từ long xuống int để khớp với Entity của ông
                feedback.SessionId = (int)session.Id;
                feedback.DiagnosticSession = session;
            }

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            // 4. Map DTO trả về
            return new FeedbackResponseDto
            {
                Id = feedback.Id,
                // ĐÃ FIX: Bỏ ?? 0 đi vì SessionId trong Entity là int (không bao giờ bị null)
                SessionId = feedback.SessionId,
                UserId = user.Id,
                Username = user.Username,
                Comments = feedback.Comments,
                CreatedAt = feedback.CreatedAt
            };
        }
    }
}