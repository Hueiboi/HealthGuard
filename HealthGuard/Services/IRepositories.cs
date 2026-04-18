using HealthGuard.Models.Dto;
using HealthGuard.Models.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthGuard.Services
{
    // Minimal repository and mapper interfaces used across services to satisfy compile-time references.
    public interface IUserRepository
    {
        Task<User> FindByIdAsync(int id);
        Task<User> FindByEmailOrUsernameAsync(string email, string username);
        Task<User> FindByUsernameAsync(string username);
        Task<User> SaveAsync(User user);
        Task<IEnumerable<User>> FindAllWithPaginationAndSearchAsync(int page, int size, string keyword);
    }

    public interface IUserMapper
    {
        UserResponseDto ToDto(User user);
    }

    public interface IRoleRepository
    {
        Task<Role> FindByRoleNameAsync(string roleName);
        Task<Role> FindByIdAsync(int id);
        Task<IEnumerable<Role>> FindAllAsync();
        Task<Role> SaveAsync(Role role);
        Task DeleteAsync(Role role);
    }

    public interface IRoleMapper { RoleDto ToDto(Role r); Role ToEntity(RoleDto dto); void UpdateEntityFromDto(RoleDto dto, Role role); }

    public interface IDiseaseRepository
    {
        Task<Disease> FindByIdAsync(int id);
        Task<Disease> SaveAsync(Disease disease);
        Task<IEnumerable<Disease>> FindAllWithPaginationAndSearchAsync(int page, int size, string keyword);
        Task DeleteAsync(Disease disease);
    }

    // Disease mapper defined further below with full members.

    public interface IPatientMapper
    {
        PatientProfileDto ToDto(Patient p);
        void UpdateEntityFromDto(PatientProfileDto dto, Patient entity);
    }

    public interface IDiseaseSymptomRepository
    {
        Task<DiseaseSymptom> FindByDiseaseIdAndSymptomIdAsync(int diseaseId, int symptomId);
        Task<IEnumerable<DiseaseSymptom>> FindByDiseaseIdAsync(int diseaseId);
        Task<DiseaseSymptom> SaveAsync(DiseaseSymptom ds);
    }

    public interface ISymptomRepository
    {
        Task<Symptom> FindByIdAsync(int id);
        Task<Symptom> SaveAsync(Symptom symptom);
        Task<IEnumerable<Symptom>> FindAllWithPaginationAndSearchAsync(int page, int size, string keyword);
        Task DeleteAsync(Symptom symptom);
    }

    public interface ISymptomMapper
    {
        SymptomDto ToDto(Symptom symptom);
        Symptom ToEntity(SymptomDto dto);
        void UpdateEntityFromDto(SymptomDto dto, Symptom entity);
    }

    // single IDiseaseMapper definition kept earlier to avoid duplicate declarations

    public interface IDiseaseMapper
    {
        DiseaseDTO ToDto(Disease d);
        Disease ToEntity(DiseaseDTO dto);
        void UpdateEntityFromDto(DiseaseDTO dto, Disease entity);
    }


    public interface IDiagnosticSessionRepository
    {
        Task<DiagnosticSession> SaveAsync(DiagnosticSession session);
        Task<IEnumerable<DiagnosticSession>> FindByUserUsernameOrderByCreatedAtDescAsync(string username, int page, int size);
        Task<DiagnosticSession> FindByIdAndUserUsernameAsync(int id, string username);
        Task<DiagnosticSession> FindByIdAsync(int id);
    }

    public interface ISessionSymptomRepository
    {
        Task<SessionSymptom> SaveAsync(SessionSymptom ss);
        Task DeleteAsync(SessionSymptom ss);
        Task<IEnumerable<SessionSymptom>> FindBySessionIdAsync(int sessionId);
    }

    public interface IDiagnosisResultRepository
    {
        Task<DiagnosisResult> SaveAsync(DiagnosisResult dr);
        Task<IEnumerable<DiagnosisResult>> FindBySessionIdAsync(int sessionId);
    }

    public interface IPatientRepository
    {
        Task<Patient> SaveAsync(Patient patient);
        Task<Patient> FindByUserUsernameAsync(string username);
    }

    public interface IFeedbackRepository
    {
        Task<Feedback> SaveAsync(Feedback feedback);
        Task<IEnumerable<Feedback>> FindAllWithPaginationAndSearchAsync(int page, int size, string keyword);
        Task DeleteAsync(Feedback feedback);
        Task<Feedback> FindByIdAsync(int id);
        Task<IEnumerable<Feedback>> FindAllByOrderByCreatedAtDescAsync(int page, int size);
    }

    public interface IFeedbackMapper
    {
        FeedbackResponseDto ToDto(Feedback feedback);
        Feedback ToEntity(FeedbackRequestDto dto);
        void UpdateEntityFromDto(FeedbackRequestDto dto, Feedback entity);
    }

    public interface IJwtUtils
    {
        string GenerateJwtToken(User user);
    }
}
