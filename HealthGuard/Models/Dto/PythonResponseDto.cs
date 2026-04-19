using System.Text.Json.Serialization;

namespace HealthGuard.Models.Dto
{
    public class PythonResponseDto
    {
        [JsonPropertyName("disease_code")]
        public string DiseaseCode { get; set; }

        [JsonPropertyName("probability")]
        public float Probability { get; set; }
    }
}