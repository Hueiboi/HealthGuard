using HealthGuard.Models.Dto;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HealthGuard.Data; 

namespace HealthGuard.Services
{
    public class AdminFeedbackService
    {
        private readonly HealthContext _context;

        public AdminFeedbackService(HealthContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<FeedbackResponseDto>> GetAllFeedbacksAsync(int page, int size)
        {
            var feedbacks = await _context.Feedbacks
                .Include(f => f.User) 
                .OrderByDescending(f => f.CreatedAt)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(f => new FeedbackResponseDto
                {
                    Id = f.Id,
                    SessionId = f.SessionId, 
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
            var feedback = await _context.Feedbacks
                .Include(f => f.User)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (feedback == null)
            {
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