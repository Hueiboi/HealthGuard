using System;

namespace HealthGuard.Models.Dto
{
    public class FeedbackResponseDto
    {
        public long Id { get; set; }
        public long SessionId { get; set; }
        public long UserId { get; set; }
        public string Username { get; set; }
        public string Comments { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}