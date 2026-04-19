using System.Collections.Generic;

namespace HealthGuard.Models.Dto
{
    public class DiagnosticResponseDto
    {
        public long SessionId { get; set; }

        // Khởi tạo sẵn List để tránh lỗi NullReference
        public List<ResultResponseDto> Results { get; set; } = new List<ResultResponseDto>();
    }
}