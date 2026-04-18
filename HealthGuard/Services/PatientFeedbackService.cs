using HealthGuard.Mappers;
using HealthGuard.Models.Dto;
using HealthGuard.Models.DTOs;
using HealthGuard.Models.Entities;
using HealthGuard.Models.Entity;
using HealthGuard.Repositories;
using System;
using System.Threading.Tasks;

namespace HealthGuard.Services
{
    public class PatientFeedbackService
    {
        private readonly IFeedbackRepository _feedbackRepository;
        private readonly IUserRepository _userRepository;
        private readonly IDiagnosticSessionRepository _sessionRepository;
        private readonly IFeedbackMapper _feedbackMapper;

        public PatientFeedbackService(
            IFeedbackRepository feedbackRepository,
            IUserRepository userRepository,
            IDiagnosticSessionRepository sessionRepository,
            IFeedbackMapper feedbackMapper)
        {
            _feedbackRepository = feedbackRepository;
            _userRepository = userRepository;
            _sessionRepository = sessionRepository;
            _feedbackMapper = feedbackMapper;
        }

        public async Task<FeedbackResponseDTO> SubmitFeedbackAsync(string username, FeedbackRequestDTO request)
        {
            // 1. Tìm user đang đăng nhập
            var user = await _userRepository.FindByUsernameAsync(username);
            if (user == null)
            {
                throw new Exception("Lỗi xác thực người dùng!");
            }

            // 2. Khởi tạo đối tượng Feedback mới
            var feedback = new Feedback
            {
                User = user,
                Comments = request.Comments
            };

            // 3. Nếu có gửi kèm sessionId, kiểm tra quyền sở hữu
            // Giả sử request.SessionId là kiểu long? (nullable)
            if (request.SessionId.HasValue)
            {
                var session = await _sessionRepository.FindByIdAndUserUsernameAsync(request.SessionId.Value, username);
                if (session == null)
                {
                    throw new Exception("Phiên khám không tồn tại hoặc không thuộc về bạn!");
                }

                feedback.DiagnosticSession = session;
            }

            // 4. Lưu xuống Database
            var savedFeedback = await _feedbackRepository.SaveAsync(feedback);

            return _feedbackMapper.ToDTO(savedFeedback);
        }
    }
}