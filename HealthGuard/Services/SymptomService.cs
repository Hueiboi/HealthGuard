using HealthGuard.Models.Dto;
using HealthGuard.Models.Entity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthGuard.Services
{
    public class SymptomService
    {
        private readonly IDiseaseSymptomRepository _diseaseSymptomRepository;
        private readonly IDiseaseRepository _diseaseRepository;
        private readonly ISymptomRepository _symptomRepository;
        private readonly ISymptomMapper _symptomMapper; // Thêm Mapper

        public SymptomService(
            IDiseaseSymptomRepository dsRepo,
            IDiseaseRepository dRepo,
            ISymptomRepository sRepo,
            ISymptomMapper sMapper)
        {
            _diseaseSymptomRepository = dsRepo;
            _diseaseRepository = dRepo;
            _symptomRepository = sRepo;
            _symptomMapper = sMapper;
        }

        public async Task<SymptomDto> CreateSymptomAsync(SymptomDto request)
        {
            var symptom = _symptomMapper.ToEntity(request);
            var saved = await _symptomRepository.SaveAsync(symptom);
            return _symptomMapper.ToDto(saved);
        }

        public async Task<IEnumerable<SymptomDto>> GetAllSymptomsAsync(int page, int size, string keyword)
        {
            var symptoms = await _symptomRepository.FindAllWithPaginationAndSearchAsync(page, size, keyword);
            var dtos = new List<SymptomDto>();
            foreach (var s in symptoms) dtos.Add(_symptomMapper.ToDto(s));
            return dtos;
        }

        public async Task<SymptomDto> GetSymptomByIdAsync(int id) // Đổi long -> int
        {
            var symptom = await _symptomRepository.FindByIdAsync(id);
            if (symptom == null) throw new Exception("Không tìm thấy triệu chứng");
            return _symptomMapper.ToDto(symptom);
        }

        public async Task<SymptomDto> UpdateSymptomAsync(int id, SymptomDto request) // Đổi long -> int
        {
            var existing = await _symptomRepository.FindByIdAsync(id);
            if (existing == null) throw new Exception("Không tìm thấy triệu chứng");
            _symptomMapper.UpdateEntityFromDto(request, existing);
            var updated = await _symptomRepository.SaveAsync(existing);
            return _symptomMapper.ToDto(updated);
        }

        public async Task DeleteSymptomAsync(int id) // Đổi long -> int
        {
            var symptom = await _symptomRepository.FindByIdAsync(id);
            if (symptom != null) await _symptomRepository.DeleteAsync(symptom);
        }

        public async Task AssignWeightScoreAsync(WeightRuleDto rule)
        {
            var disease = await _diseaseRepository.FindByIdAsync(rule.DiseaseId); // Giả định DTO trả về kiểu int
            var symptom = await _symptomRepository.FindByIdAsync(rule.SymptomId); // Giả định DTO trả về kiểu int

            if (disease == null || symptom == null) throw new Exception("Thông tin không hợp lệ");

            var diseaseSymptom = await _diseaseSymptomRepository.FindByDiseaseIdAndSymptomIdAsync(rule.DiseaseId, rule.SymptomId)
                                 ?? new DiseaseSymptom();

            diseaseSymptom.Disease = disease;
            diseaseSymptom.Symptom = symptom;
            diseaseSymptom.Weight = rule.Weight;

            await _diseaseSymptomRepository.SaveAsync(diseaseSymptom);
        }

        public async Task<IEnumerable<WeightRuleDto>> GetSymptomsByDiseaseAsync(int diseaseId) // Đổi long -> int
        {
            var diseaseSymptoms = await _diseaseSymptomRepository.FindByDiseaseIdAsync(diseaseId);
            var resultList = new List<WeightRuleDto>();

            foreach (var ds in diseaseSymptoms)
            {
                resultList.Add(new WeightRuleDto()); // Tùy biến sau
            }
            return resultList;
        }
    }
}