using System.Collections.Generic;

namespace HealthGuard.Models.Dto
{
    public class DiagnosticRequestDto
    {
        // Danh sách triệu chứng hiển thị trên màn hình
        public List<SymptomInputDto> Symptoms { get; set; } = new List<SymptomInputDto>();
    }
}