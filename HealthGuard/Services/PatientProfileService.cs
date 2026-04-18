using HealthGuard.Mappers;
using HealthGuard.Models.Dto;
using HealthGuard.Models.DTOs;
using HealthGuard.Models.Entities;
using HealthGuard.Repositories;
using System;
using System.Threading.Tasks;

namespace HealthGuard.Services
{
    public class PatientProfileService
    {
        private readonly IPatientRepository _patientRepository;
        private readonly IPatientMapper _patientMapper;

        public PatientProfileService(
            IPatientRepository patientRepository,
            IPatientMapper patientMapper)
        {
            _patientRepository = patientRepository;
            _patientMapper = patientMapper;
        }

        public async Task<PatientProfileDTO> GetMyProfileAsync(string username)
        {
            var myProfile = await _patientRepository.FindByUserUsernameAsync(username);
            if (myProfile == null)
            {
                throw new Exception($"Không tìm thấy hồ sơ bệnh của người dùng: {username}");
            }

            return _patientMapper.ToDTO(myProfile);
        }

        public async Task<PatientProfileDTO> UpdateProfileAsync(PatientProfileDTO request, string username)
        {
            var existingPatient = await _patientRepository.FindByUserUsernameAsync(username);
            if (existingPatient == null)
            {
                throw new Exception($"Không tìm thấy hồ sơ bệnh của người dùng: {username}");
            }

            // Mapper sẽ copy dữ liệu từ request sang existingPatient
            _patientMapper.UpdateEntityFromDTO(request, existingPatient);

            var updatedPatient = await _patientRepository.SaveAsync(existingPatient);
            return _patientMapper.ToDTO(updatedPatient);
        }
    }
}