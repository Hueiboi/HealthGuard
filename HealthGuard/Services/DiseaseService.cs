using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HealthGuard.Models.Entity;
using HealthGuard.Models.Dto;

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

        public async Task<DiseaseDTO> CreateDiseaseAsync(DiseaseDTO request) // Dùng DiseaseDTO viết hoa theo ý bạn
        {
            var disease = _diseaseMapper.ToEntity(request);
            var savedDisease = await _diseaseRepository.SaveAsync(disease);
            return _diseaseMapper.ToDto(savedDisease);
        }

        public async Task<IEnumerable<DiseaseDTO>> GetAllDiseasesAsync(int page, int size, string keyword)
        {
            var diseases = await _diseaseRepository.FindAllWithPaginationAndSearchAsync(page, size, keyword);

            var dtos = new List<DiseaseDTO>();
            foreach (var disease in diseases)
            {
                dtos.Add(_diseaseMapper.ToDto(disease));
            }
            return dtos;
        }

        public async Task<DiseaseDTO> GetDiseaseByIdAsync(int id) // Đổi long -> int
        {
            var disease = await _diseaseRepository.FindByIdAsync(id);
            if (disease == null) throw new Exception($"Không tìm thấy bệnh lý với ID: {id}");
            return _diseaseMapper.ToDto(disease);
        }

        public async Task<DiseaseDTO> UpdateDiseaseAsync(int id, DiseaseDTO request) // Đổi long -> int
        {
            var existingDisease = await _diseaseRepository.FindByIdAsync(id);
            if (existingDisease == null) throw new Exception($"Không tìm thấy bệnh lý với ID: {id}");

            _diseaseMapper.UpdateEntityFromDto(request, existingDisease);
            var updatedDisease = await _diseaseRepository.SaveAsync(existingDisease);
            return _diseaseMapper.ToDto(updatedDisease);
        }

        public async Task DeleteDiseaseAsync(int id) // Đổi long -> int
        {
            var disease = await _diseaseRepository.FindByIdAsync(id);
            if (disease == null) throw new Exception($"Không tìm thấy bệnh lý với ID: {id}");
            await _diseaseRepository.DeleteAsync(disease);
        }
    }
}