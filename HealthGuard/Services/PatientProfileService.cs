using HealthGuard.Models.Dto;
using HealthGuard.Models.Entity;
using System;
using System.Threading.Tasks;

namespace HealthGuard.Services
{
    public class PatientProfileService
    {
        private readonly IPatientRepository _patientRepository;
        private readonly IPatientMapper _patientMapper;

        public PatientProfileService(IPatientRepository patientRepository, IPatientMapper patientMapper)
        {
            _patientRepository = patientRepository;
            _patientMapper = patientMapper;
        }

        public async Task<PatientProfileDto> GetMyProfileAsync(string username)
        {
            var myProfile = await _patientRepository.FindByUserUsernameAsync(username);
            if (myProfile == null) throw new Exception($"Không tìm thấy hồ sơ: {username}");
            return _patientMapper.ToDto(myProfile);
        }

        public async Task<PatientProfileDto> UpdateProfileAsync(PatientProfileDto request, string username)
        {
            var existingPatient = await _patientRepository.FindByUserUsernameAsync(username);
            if (existingPatient == null) throw new Exception($"Không tìm thấy hồ sơ: {username}");

            _patientMapper.UpdateEntityFromDto(request, existingPatient);
            var updatedPatient = await _patientRepository.SaveAsync(existingPatient);
            return _patientMapper.ToDto(updatedPatient);
        }
    }
}