using HealthGuard.Mappers;
using HealthGuard.Models.Dto;
using HealthGuard.Models.DTOs;
using HealthGuard.Models.Entities;
using HealthGuard.Models.Entity;
using HealthGuard.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthGuard.Services
{
    public class SymptomService
    {
        private readonly ISymptomRepository _symptomRepository;
        private readonly ISymptomMapper _symptomMapper;
        private readonly IDiseaseSymptomRepository _diseaseSymptomRepository;
        private readonly IDiseaseRepository _diseaseRepository;

        public SymptomService(
            ISymptomRepository symptomRepository,
            ISymptomMapper symptomMapper,
            IDiseaseSymptomRepository diseaseSymptomRepository,
            IDiseaseRepository diseaseRepository)
        {
            _symptomRepository = symptomRepository;
            _symptomMapper = symptomMapper;
            _diseaseSymptomRepository = diseaseSymptomRepository;
            _diseaseRepository = diseaseRepository;
        }

        public async Task<SymptomDTO> CreateSymptomAsync(SymptomDTO request)
        {
            var symptom = _symptomMapper.ToEntity(request);
            var savedSymptom = await _symptomRepository.SaveAsync(symptom);
            return _symptomMapper.ToDTO(savedSymptom);
        }

        public async Task<SymptomDTO> GetSymptomByIdAsync(long id)
        {
            var symptom = await _symptomRepository.FindByIdAsync(id);
            if (symptom == null)
            {
                throw new Exception($"Không tìm thấy triệu chứng có ID: {id}");
            }
            return _symptomMapper.ToDTO(symptom);
        }

        public async Task<IEnumerable<SymptomDTO>> GetAllSymptomsAsync(int page, int size, string keyword)
        {
            var symptoms = await _symptomRepository.FindAllWithPaginationAndSearchAsync(page, size, keyword);
            var dtos = new List<SymptomDTO>();

            foreach (var symptom in symptoms)
            {
                dtos.Add(_symptomMapper.ToDTO(symptom));
            }
            return dtos;
        }

        public async Task<SymptomDTO> UpdateSymptomAsync(long id, SymptomDTO request)
        {
            var existingSymptom = await _symptomRepository.FindByIdAsync(id);
            if (existingSymptom == null)
            {
                throw new Exception($"Không tìm thấy triệu chứng có ID: {id}");
            }

            _symptomMapper.UpdateEntityFromDTO(request, existingSymptom);
            var updatedSymptom = await _symptomRepository.SaveAsync(existingSymptom);
            return _symptomMapper.ToDTO(updatedSymptom);
        }

        public async Task DeleteSymptomAsync(long id)
        {
            var symptom = await _symptomRepository.FindByIdAsync(id);
            if (symptom == null)
            {
                throw new Exception($"Không tìm thấy triệu chứng có ID: {id}");
            }
            await _symptomRepository.DeleteAsync(symptom);
        }

        // Hàm gắn triệu chứng vào bệnh và lưu trọng số
        public async Task AssignWeightScoreAsync(WeightRuleDTO rule)
        {
            var disease = await _diseaseRepository.FindByIdAsync(rule.DiseaseId);
            if (disease == null)
            {
                throw new Exception($"Không tìm thấy bệnh với ID: {rule.DiseaseId}");
            }

            var symptom = await _symptomRepository.FindByIdAsync(rule.SymptomId);
            if (symptom == null)
            {
                throw new Exception($"Không tìm thấy triệu chứng với ID: {rule.SymptomId}");
            }

            // Kiểm tra xem luật này đã có trong DB chưa
            var diseaseSymptom = await _diseaseSymptomRepository.FindByDiseaseIdAndSymptomIdAsync(rule.DiseaseId, rule.SymptomId);

            if (diseaseSymptom == null)
            {
                // Nếu chưa có thì tạo mới
                diseaseSymptom = new DiseaseSymptom();
            }

            // Cập nhật dữ liệu
            diseaseSymptom.Disease = disease;
            diseaseSymptom.Symptom = symptom;
            diseaseSymptom.WeightScore = rule.WeightScore;

            await _diseaseSymptomRepository.SaveAsync(diseaseSymptom);
        }

        // Lấy danh sách triệu chứng cùng trọng số
        public async Task<IEnumerable<WeightRuleDTO>> GetSymptomsByDiseaseAsync(long diseaseId)
        {
            var diseaseSymptoms = await _diseaseSymptomRepository.FindByDiseaseIdAsync(diseaseId);
            var resultList = new List<WeightRuleDTO>();

            foreach (var ds in diseaseSymptoms)
            {
                resultList.Add(new WeightRuleDTO
                {
                    DiseaseId = ds.Disease.Id,
                    SymptomId = ds.Symptom.Id,
                    WeightScore = ds.WeightScore
                });
            }
            return resultList;
        }
    }
}