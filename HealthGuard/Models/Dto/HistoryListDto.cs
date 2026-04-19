using System;

namespace HealthGuard.Models.Dto
{
    public class HistoryListDto
    {
        public long SessionId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; }
    }
}