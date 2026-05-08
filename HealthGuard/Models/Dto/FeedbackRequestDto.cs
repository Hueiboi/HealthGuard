using System.ComponentModel.DataAnnotations;

namespace HealthGuard.Models.Dto
{
    public class FeedbackRequestDto
    {
        public long? SessionId { get; set; }

        [Required(ErrorMessage = "Nội dung phản hồi không được để trống.")]
        [StringLength(1000, ErrorMessage = "Nội dung phản hồi không được quá 1000 ký tự.")]
        public string Comments { get; set; }
    }
}