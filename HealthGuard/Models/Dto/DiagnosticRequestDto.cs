using System.Collections.Generic;

namespace HealthGuard.Models.Dto
{
    public class DiagnosticRequestDto
    {
        public List<SymptomInputDto> SelectedSymptoms { get; set; } = new List<SymptomInputDto>();
    }

}