using System.Collections.Generic;

namespace HealthGuard.Models.Entity
{
    public class Symptom
    {
        public long Id { get; set; }

        // THÊM DÒNG NÀY ĐỂ FIX LỖI THIẾU SYMPTOMNAME
        public string SymptomName { get; set; }

        // Nếu có các quan hệ (Navigation properties) thì giữ nguyên
        public virtual ICollection<DiseaseSymptom> DiseaseSymptoms { get; set; }
        public virtual ICollection<SessionSymptom> SessionSymptoms { get; set; }
    }
}