using HealthGuard.Mappers;
using HealthGuard.Models.Dto;
using HealthGuard.Models.DTOs;
using HealthGuard.Models.Entities;
using HealthGuard.Repositories;
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

        public async Task<IEnumerable<FeedbackResponseDTO>> GetAllFeedbacksAsync(int page, int size)
        {
            // Repository thực hiện logic: OrderByDescending(f => f.CreatedAt).Skip(...).Take(...)
            var feedbacks = await _feedbackRepository.FindAllByOrderByCreatedAtDescAsync(page, size);

            var dtos = new List<FeedbackResponseDTO>();
            foreach (var feedback in feedbacks)
            {
                dtos.Add(_feedbackMapper.ToDTO(feedback));
            }

            return dtos;
        }

        public async Task<FeedbackResponseDTO> GetFeedbackByIdAsync(long id)
        {
            var feedback = await _feedbackRepository.FindByIdAsync(id);
            if (feedback == null)
            {
                throw new Exception($"Không tìm thấy phản hồi với ID: {id}");
            }

            return _feedbackMapper.ToDTO(feedback);
        }
    }
}