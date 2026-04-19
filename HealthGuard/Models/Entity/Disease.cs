namespace HealthGuard.Models.Entity
{
    public class Disease
    {
        public long Id { get; set; }
        public string? DiseaseCode { get; set; }
        public string DiseaseName { get; set; } // THÊM DÒNG NÀY
        public string? Description { get; set; }
        public string TreatmentAdvice { get; set; }
    }
}