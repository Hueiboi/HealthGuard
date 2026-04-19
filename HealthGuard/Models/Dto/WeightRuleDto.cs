namespace HealthGuard.Models.Dto
{
    public class WeightRuleDto
    {
        public long DiseaseId { get; set; }
        public long SymptomId { get; set; }
        public double WeightScore { get; set; }
    }
}