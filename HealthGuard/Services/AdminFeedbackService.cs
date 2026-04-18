using HealthGuard.Models.Dto;
using HealthGuard.Models.Entity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthGuard.Services
{
    public class AdminFeedbackService
    {
        private readonly IFeedbackRepository _feedbackRepository;
        private readonly IFeedbackMapper _feedbackMapper;

        public AdminFeedbackService(
            IFeedbackRepository feedbackRepository,
            IFeedbackMapper feedbackMapper)
        {
            _feedbackRepository = feedbackRepository;
            _feedbackMapper = feedbackMapper;
        }

        public async Task<IEnumerable<FeedbackResponseDto>> GetAllFeedbacksAsync(int page, int size)
        {
            var feedbacks = await _feedbackRepository.FindAllByOrderByCreatedAtDescAsync(page, size);

            var dtos = new List<FeedbackResponseDto>();
            foreach (var feedback in feedbacks)
            {
                dtos.Add(_feedbackMapper.ToDto(feedback));
            }

            return dtos;
        }

        public async Task<FeedbackResponseDto> GetFeedbackByIdAsync(int id) // Đổi long -> int
        {
            var feedback = await _feedbackRepository.FindByIdAsync(id);
            if (feedback == null)
            {
                throw new Exception($"Không tìm thấy phản hồi với ID: {id}");
            }

            return _feedbackMapper.ToDto(feedback);
        }
    }
}