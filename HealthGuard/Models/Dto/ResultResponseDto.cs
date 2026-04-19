namespace HealthGuard.Models.Dto
{
    public class ResultResponseDto
    {
        public string DiseaseName { get; set; }
        public float ProbabilityPercentage { get; set; }
        public string TreatmentAdvice { get; set; }
    }
}