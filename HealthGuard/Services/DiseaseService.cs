using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HealthGuard.Models.Entities;
using HealthGuard.Models.DTOs;
using HealthGuard.Repositories;
using HealthGuard.Mappers;

namespace HealthGuard.Services
{
    public class DiseaseService
    {
        private readonly IDiseaseRepository _diseaseRepository;
        private readonly IDiseaseMapper _diseaseMapper;

        public DiseaseService(IDiseaseRepository diseaseRepository, IDiseaseMapper diseaseMapper)
        {
            _diseaseRepository = diseaseRepository;
            _diseaseMapper = diseaseMapper;
        }

        public async Task<DiseaseDTO> CreateDiseaseAsync(DiseaseDTO request)
        {
            var disease = _diseaseMapper.ToEntity(request);
            var savedDisease = await _diseaseRepository.SaveAsync(disease);
            return _diseaseMapper.ToDTO(savedDisease);
        }

        public async Task<IEnumerable<DiseaseDTO>> GetAllDiseasesAsync(int page, int size, string keyword)
        {
            // Trong EF Core, tìm kiếm không phân biệt hoa thường có thể dùng string.Contains hoặc EF.Functions.Like
            var diseases = await _diseaseRepository.FindAllWithPaginationAndSearchAsync(page, size, keyword);

            var dtos = new List<DiseaseDTO>();
            foreach (var disease in diseases)
            {
                dtos.Add(_diseaseMapper.ToDTO(disease));
            }
            return dtos;
        }

        public async Task<DiseaseDTO> GetDiseaseByIdAsync(long id)
        {
            var disease = await _diseaseRepository.FindByIdAsync(id);
            if (disease == null)
            {
                throw new Exception($"Không tìm thấy bệnh lý với ID: {id}");
            }
            return _diseaseMapper.ToDTO(disease);
        }

        public async Task<DiseaseDTO> UpdateDiseaseAsync(long id, DiseaseDTO request)
        {
            var existingDisease = await _diseaseRepository.FindByIdAsync(id);
            if (existingDisease == null)
            {
                throw new Exception($"Không tìm thấy bệnh lý với ID: {id}");
            }

            // Map dữ liệu mới vào entity cũ
            _diseaseMapper.UpdateEntityFromDTO(request, existingDisease);

            var updatedDisease = await _diseaseRepository.SaveAsync(existingDisease);
            return _diseaseMapper.ToDTO(updatedDisease);
        }

        public async Task DeleteDiseaseAsync(long id)
        {
            var disease = await _diseaseRepository.FindByIdAsync(id);
            if (disease == null)
            {
                throw new Exception($"Không tìm thấy bệnh lý với ID: {id}");
            }
            await _diseaseRepository.DeleteAsync(disease);
        }
    }
}