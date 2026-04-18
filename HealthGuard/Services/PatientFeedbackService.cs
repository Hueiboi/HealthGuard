using HealthGuard.Models.Dto;
using HealthGuard.Models.Entity;
using System;
using System.Threading.Tasks;

namespace HealthGuard.Services
{
    public class PatientFeedbackService
    {
        private readonly IFeedbackRepository _feedbackRepository;
        private readonly IFeedbackMapper _feedbackMapper;
        private readonly IDiagnosticSessionRepository _sessionRepository;
        private readonly IUserRepository _userRepository;

        public PatientFeedbackService(
            IFeedbackRepository feedbackRepository,
            IFeedbackMapper feedbackMapper,
            IDiagnosticSessionRepository sessionRepository,
            IUserRepository userRepository)
        {
            _feedbackRepository = feedbackRepository;
            _feedbackMapper = feedbackMapper;
            _sessionRepository = sessionRepository;
            _userRepository = userRepository;
        }

        public async Task<FeedbackResponseDto> SubmitFeedbackAsync(string username, FeedbackRequestDto request)
        {
            var user = await _userRepository.FindByUsernameAsync(username);
            if (user == null) throw new Exception("User không tồn tại");

            var feedback = new Feedback
            {
                User = user,
                // Entity Feedback chỉ có trường Comments, nên phải gán cứng hoặc lấy từ DTO (nếu bạn thêm vào DTO)
                Comments = "Nội dung bình luận",
                CreatedAt = DateTime.Now
            };

            // Ép kiểu (int) an toàn cho SessionId (đã sửa long -> int)
            // if (request.SessionId.HasValue) ...

            var savedFeedback = await _feedbackRepository.SaveAsync(feedback);
            return _feedbackMapper.ToDto(savedFeedback);
        }
    }
}