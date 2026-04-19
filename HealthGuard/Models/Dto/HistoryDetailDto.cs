using System;
using System.Collections.Generic;

namespace HealthGuard.Models.Dto
{
    public class HistoryDetailDto
    {
        public long SessionId { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<SymptomItem> Symptoms { get; set; } = new List<SymptomItem>();
        public List<ResultItem> Results { get; set; } = new List<ResultItem>();

        public class SymptomItem
        {
            public string SymptomName { get; set; }
            public int DurationDays { get; set; }
            public string SeverityLevel { get; set; }
        }

        public class ResultItem
        {
            public string DiseaseName { get; set; }
            public float ProbabilityPercentage { get; set; }
            public string TreatmentAdvice { get; set; }
        }
    }
}