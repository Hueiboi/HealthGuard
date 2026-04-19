using HealthGuard.Data; // Thay bằng namespace chứa HealthContext của ông
using HealthGuard.Models.Dto;
using HealthGuard.Models.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthGuard.Services
{
    public class DiseaseService
    {
        private readonly HealthContext _context;

        // Tiêm trực tiếp HealthContext, dọn dẹp Repo và Mapper cũ
        public DiseaseService(HealthContext context)
        {
            _context = context;
        }

        public async Task<DiseaseDto> CreateDiseaseAsync(DiseaseDto request)
        {
            var disease = new Disease
            {
                DiseaseCode = request.DiseaseCode,
                DiseaseName = request.DiseaseName,
                TreatmentAdvice = request.TreatmentAdvice
            };

            _context.Diseases.Add(disease);
            await _context.SaveChangesAsync(); // Lưu xuống DB để lấy Id tự sinh

            request.Id = disease.Id;
            return request;
        }

        public async Task<IEnumerable<DiseaseDto>> GetAllDiseasesAsync(int page, int size, string keyword)
        {
            var query = _context.Diseases.AsQueryable();

            // Xử lý tìm kiếm bằng LINQ
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string lowerKeyword = keyword.ToLower();
                query = query.Where(d => d.DiseaseName.ToLower().Contains(lowerKeyword) ||
                                         d.DiseaseCode.ToLower().Contains(lowerKeyword));
            }

            // Phân trang và map trực tiếp sang DTO
            return await query
                .OrderBy(d => d.DiseaseName)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(d => new DiseaseDto
                {
                    Id = d.Id,
                    DiseaseCode = d.DiseaseCode,
                    DiseaseName = d.DiseaseName,
                    TreatmentAdvice = d.TreatmentAdvice
                })
                .ToListAsync();
        }

        public async Task<DiseaseDto> GetDiseaseByIdAsync(long id)
        {
            var disease = await _context.Diseases.FindAsync(id);
            if (disease == null)
            {
                // Dùng Exception chuẩn của .NET
                throw new KeyNotFoundException($"Không tìm thấy bệnh lý với ID: {id}");
            }

            return new DiseaseDto
            {
                Id = disease.Id,
                DiseaseCode = disease.DiseaseCode,
                DiseaseName = disease.DiseaseName,
                TreatmentAdvice = disease.TreatmentAdvice
            };
        }

        public async Task<DiseaseDto> UpdateDiseaseAsync(long id, DiseaseDto request)
        {
            var existingDisease = await _context.Diseases.FindAsync(id);
            if (existingDisease == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy bệnh lý với ID: {id}");
            }

            // Cập nhật dữ liệu (Thay cho Mapper)
            existingDisease.DiseaseCode = request.DiseaseCode;
            existingDisease.DiseaseName = request.DiseaseName;
            existingDisease.TreatmentAdvice = request.TreatmentAdvice;

            await _context.SaveChangesAsync(); // EF Core tự theo dõi và cập nhật

            return new DiseaseDto
            {
                Id = existingDisease.Id,
                DiseaseCode = existingDisease.DiseaseCode,
                DiseaseName = existingDisease.DiseaseName,
                TreatmentAdvice = existingDisease.TreatmentAdvice
            };
        }

        public async Task DeleteDiseaseAsync(long id)
        {
            var disease = await _context.Diseases.FindAsync(id);
            if (disease == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy bệnh lý với ID: {id}");
            }

            _context.Diseases.Remove(disease);
            await _context.SaveChangesAsync();
        }
    }
}