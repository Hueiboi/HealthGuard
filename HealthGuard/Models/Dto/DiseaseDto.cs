namespace HealthGuard.Models.Dto
{
    public class DiseaseDto
    {
        public long Id { get; set; }
        public string DiseaseCode { get; set; }
        public string DiseaseName { get; set; }
        public string TreatmentAdvice { get; set; }
    }
}