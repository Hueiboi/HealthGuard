using HealthGuard.Data;
using HealthGuard.Models.Dto;
using HealthGuard.Models.Entity; // Chuẩn folder Entity số ít của ông
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthGuard.Services
{
    public class SymptomService
    {
        private readonly HealthContext _context;

        public SymptomService(HealthContext context)
        {
            _context = context;
        }

        public async Task<SymptomDto> CreateSymptomAsync(SymptomDto request)
        {
            var symptom = new Symptom { SymptomName = request.SymptomName };
            _context.Symptoms.Add(symptom);
            await _context.SaveChangesAsync();

            request.Id = symptom.Id;
            return request;
        }

        public async Task<SymptomDto> GetSymptomByIdAsync(long id)
        {
            var symptom = await _context.Symptoms.FindAsync(id);
            if (symptom == null) throw new KeyNotFoundException($"Không tìm thấy triệu chứng ID: {id}");

            return new SymptomDto { Id = symptom.Id, SymptomName = symptom.SymptomName };
        }

        public async Task<IEnumerable<SymptomDto>> GetAllSymptomsAsync(int page, int size, string keyword)
        {
            var query = _context.Symptoms.AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(s => s.SymptomName.ToLower().Contains(keyword.ToLower()));

            // Nếu phân trang page bắt đầu từ 1, thì (page - 1) * size. 
            // Nếu API của ông truyền page từ 0 thì ông sửa thành page * size nhé.
            return await query
                .OrderBy(s => s.SymptomName)
                .Skip((page > 0 ? page - 1 : 0) * size)
                .Take(size)
                .Select(s => new SymptomDto { Id = s.Id, SymptomName = s.SymptomName })
                .ToListAsync();
        }

        // ==========================================
        // ĐÃ THÊM LẠI: Hàm cập nhật Triệu chứng (bị thiếu)
        // ==========================================
        public async Task<SymptomDto> UpdateSymptomAsync(long id, SymptomDto request)
        {
            var existingSymptom = await _context.Symptoms.FindAsync(id);
            if (existingSymptom == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy triệu chứng với ID: {id}");
            }

            existingSymptom.SymptomName = request.SymptomName;

            // EF Core tự track thay đổi và sinh lệnh UPDATE
            await _context.SaveChangesAsync();

            return new SymptomDto { Id = existingSymptom.Id, SymptomName = existingSymptom.SymptomName };
        }

        // ==========================================
        // ĐÃ THÊM LẠI: Hàm xóa Triệu chứng (bị thiếu)
        // ==========================================
        public async Task DeleteSymptomAsync(long id)
        {
            var symptom = await _context.Symptoms.FindAsync(id);
            if (symptom == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy triệu chứng với ID: {id}");
            }

            _context.Symptoms.Remove(symptom);
            await _context.SaveChangesAsync();
        }

        public async Task AssignWeightScoreAsync(WeightRuleDto rule)
        {
            var disease = await _context.Diseases.FindAsync(rule.DiseaseId)
                ?? throw new KeyNotFoundException("Không tìm thấy bệnh.");
            var symptom = await _context.Symptoms.FindAsync(rule.SymptomId)
                ?? throw new KeyNotFoundException("Không tìm thấy triệu chứng.");

            var diseaseSymptom = await _context.DiseaseSymptoms
                .FirstOrDefaultAsync(ds => ds.DiseaseId == rule.DiseaseId && ds.SymptomId == rule.SymptomId);

            if (diseaseSymptom == null)
            {
                diseaseSymptom = new DiseaseSymptom { DiseaseId = (int)rule.DiseaseId, SymptomId = (int)rule.SymptomId };
                _context.DiseaseSymptoms.Add(diseaseSymptom);
            }

            // Ép kiểu (float) để khớp với Entity
            diseaseSymptom.WeightScore = rule.WeightScore;

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<WeightRuleDto>> GetSymptomsByDiseaseAsync(long diseaseId)
        {
            return await _context.DiseaseSymptoms
                .Where(ds => ds.DiseaseId == diseaseId)
                .Select(ds => new WeightRuleDto
                {
                    DiseaseId = ds.DiseaseId,
                    SymptomId = ds.SymptomId,
                    WeightScore = ds.WeightScore
                }).ToListAsync();
        }
    }
}