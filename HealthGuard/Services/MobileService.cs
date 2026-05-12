using HealthGuard.Data;
using HealthGuard.Models.Dto;
using HealthGuard.Models.Entity;
using HealthGuard.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HealthGuard.Services
{
    public class MobileService
    {
        private readonly HealthContext _context;
        private readonly PatientProfileService _patientProfileService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IJwtUtils _jwtUtils;
        private readonly IMemoryCache _cache;

        public MobileService(
            HealthContext context,
            PatientProfileService patientProfileService,
            IHttpClientFactory httpClientFactory,
            IJwtUtils jwtUtils,
            IMemoryCache cache)
        {
            _context = context;
            _patientProfileService = patientProfileService;
            _httpClientFactory = httpClientFactory;
            _jwtUtils = jwtUtils;
            _cache = cache;
        }

        // ================== DASHBOARD ==================
        public async Task<MobileDashboardDto> GetDashboardDataAsync(string username)
        {
            var profile = await _patientProfileService.GetMyProfileAsync(username);
            double bmi = 0;
            string bmiStatus = "Chưa có dữ liệu";
            if (profile.Height > 0 && profile.Weight > 0)
            {
                double heightInMeters = profile.Height / 100.0;
                bmi = Math.Round(profile.Weight / Math.Pow(heightInMeters, 2), 1);
                if (bmi < 18.5) bmiStatus = "Gầy";
                else if (bmi < 25) bmiStatus = "Bình thường";
                else if (bmi < 30) bmiStatus = "Thừa cân";
                else bmiStatus = "Béo phì";
            }

            var latestSession = await _context.DiagnosticSessions
                .Include(s => s.DiagnosisResults).ThenInclude(dr => dr.Disease)
                .Where(s => s.User.Username == username)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync();

            string topDiseaseName = "Chưa có chẩn đoán";
            string diagnosisScore = "--%";

            if (latestSession != null && latestSession.DiagnosisResults.Any())
            {
                var topResult = latestSession.DiagnosisResults.OrderByDescending(r => r.ProbabilityPercentage).First();
                topDiseaseName = topResult.Disease.DiseaseName;
                diagnosisScore = $"{topResult.ProbabilityPercentage}%";
            }

            return new MobileDashboardDto { Bmi = bmi > 0 ? bmi.ToString() : "--", BmiStatus = bmiStatus, TopDiseaseName = topDiseaseName, DiagnosisScore = diagnosisScore };
        }

        // ================== CHẨN ĐOÁN AI ==================
        public async Task<List<PythonDiagnosis>> ProcessDiagnosisAsync(string username, MobileDiagnosticRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) throw new Exception("Không tìm thấy thông tin người dùng.");

            var pythonPayload = new { selectedSymptoms = request.SymptomIds.Select(id => new { symptomId = id }).ToList() };
            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var jsonContent = new StringContent(JsonSerializer.Serialize(pythonPayload, jsonOptions), Encoding.UTF8, "application/json");

            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsync("http://127.0.0.1:5000/predict", jsonContent);

            if (!response.IsSuccessStatusCode) throw new Exception("Lỗi khi gọi mô hình AI Python.");

            var pythonResultString = await response.Content.ReadAsStringAsync();
            var pythonData = JsonSerializer.Deserialize<PythonAiResponse>(pythonResultString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (pythonData?.Diagnoses == null || !pythonData.Diagnoses.Any())
                throw new Exception("AI không trả về kết quả chẩn đoán nào.");

            // ==========================================
            // BƯỚC 1: LƯU PHIÊN CHẨN ĐOÁN
            // ==========================================
            var newSession = new DiagnosticSession
            {
                User = user,
                Status = "Hoàn tất",
                CreatedAt = DateTime.Now,
                MainSymptomDescription = string.IsNullOrEmpty(request.MainSymptomDescription) ? "Không có mô tả thêm" : request.MainSymptomDescription,
                PainLevel = request.PainLevel
            };
            _context.DiagnosticSessions.Add(newSession);
            await _context.SaveChangesAsync();

            // ==========================================
            // BƯỚC 2: LƯU TRIỆU CHỨNG 
            // ==========================================
            var sessionSymptoms = request.SymptomIds.Select(symId => new SessionSymptom
            {
                DiagnosticSessionId = newSession.Id,
                SymptomId = symId,
                DurationDays = 1,
                SeverityLevel = "Chưa xác định"
            });
            _context.SessionSymptoms.AddRange(sessionSymptoms);
            await _context.SaveChangesAsync();

            // ==========================================
            // BƯỚC 3: LỌC TOP 5 BỆNH VÀ LƯU KẾT QUẢ
            // ==========================================
            var topDiagnoses = pythonData.Diagnoses
                .Where(d => d.Probability > 0)
                .OrderByDescending(d => d.Probability)
                .Take(5)
                .ToList();

            var aiResults = new List<DiagnosisResult>();
            foreach (var diag in topDiagnoses)
            {
                if (string.IsNullOrEmpty(diag.DiseaseName)) continue;

                var disease = await _context.Diseases.FirstOrDefaultAsync(d => d.DiseaseName == diag.DiseaseName);

                if (disease == null)
                {
                    disease = new Disease
                    {
                        DiseaseCode = "AI-" + Guid.NewGuid().ToString().Substring(0, 6).ToUpper(),
                        DiseaseName = diag.DiseaseName,
                        Description = string.IsNullOrEmpty(diag.Description) ? "Đang cập nhật" : diag.Description,
                        TreatmentAdvice = string.IsNullOrEmpty(diag.Treatment) ? "Đang cập nhật" : diag.Treatment
                    };
                    _context.Diseases.Add(disease);
                    await _context.SaveChangesAsync();
                }

                aiResults.Add(new DiagnosisResult { SessionId = newSession.Id, DiseaseId = disease.Id, ProbabilityPercentage = diag.Probability });
            }

            if (aiResults.Any())
            {
                _context.DiagnosisResults.AddRange(aiResults);
                await _context.SaveChangesAsync();
            }

            return topDiagnoses;
        }

        // ================== AUTH / ĐĂNG KÝ / OTP ==================
        public async Task<UserResponseDto> MobileRegisterAsync(MobileRegisterRequestDto request)
        {
            if (await _context.Users.AnyAsync(u => u.PhoneNumber == request.PhoneNumber))
                throw new InvalidOperationException("Số điện thoại này đã được đăng ký!");

            string generatedUsername = $"M_{request.PhoneNumber}";
            string randomPassword = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString());
            string emailFallback = string.IsNullOrEmpty(request.Email) ? $"{generatedUsername}@vitalis.local" : request.Email;

            var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "ROLE_USER")
                            ?? throw new InvalidOperationException("Lỗi hệ thống: Không tìm thấy quyền ROLE_USER");

            var newUser = new User
            {
                Username = generatedUsername,
                Email = emailFallback,
                PhoneNumber = request.PhoneNumber,
                Password = randomPassword,
                Role = userRole,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
            };
            _context.Users.Add(newUser);

            var newPatient = new Patient { User = newUser, FullName = request.FullName, DateOfBirth = request.DateOfBirth, MedicalHistory = null };
            _context.Patients.Add(newPatient);

            await _context.SaveChangesAsync();

            return new UserResponseDto { Id = newUser.Id, Username = newUser.Username, Email = newUser.Email, RoleName = newUser.Role.RoleName, IsActive = newUser.IsActive, CreatedAt = newUser.CreatedAt };
        }

        // 👉 ĐÃ SỬA HÀM NÀY ĐỂ TRẢ VỀ CHUỖI MÃ OTP
        public async Task<string> SendOtpAsync(string phoneNumber)
        {
            var userExists = await _context.Users.AnyAsync(u => u.PhoneNumber == phoneNumber);
            if (!userExists) throw new InvalidOperationException("Số điện thoại chưa được đăng ký. Vui lòng đăng ký tài khoản!");

            var random = new Random();
            string otpCode = random.Next(100000, 999999).ToString();

            var cacheOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(60));
            _cache.Set($"OTP_{phoneNumber}", otpCode, cacheOptions);

            Console.WriteLine($"\n[MOBILE APP] Ma OTP cua {phoneNumber} la: {otpCode}\n");

            // Ép trả mã OTP ra ngoài
            return otpCode;
        }

        public async Task<(string Token, string FullName)> VerifyOtpAsync(string phoneNumber, string otpCode)
        {
            if (_cache.TryGetValue($"OTP_{phoneNumber}", out string savedOtp))
            {
                if (savedOtp == otpCode)
                {
                    _cache.Remove($"OTP_{phoneNumber}");

                    var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
                    if (user == null) throw new UnauthorizedAccessException("Số điện thoại chưa được đăng ký! Vui lòng đăng ký tài khoản.");
                    if (!user.IsActive) throw new UnauthorizedAccessException("Tài khoản của bạn đã bị khóa!");

                    var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == user.Id);
                    string fullName = patient != null ? patient.FullName : "Khách";

                    string token = _jwtUtils.GenerateJwtToken(user);
                    return (token, fullName);
                }
                throw new UnauthorizedAccessException("Mã OTP không chính xác!");
            }
            throw new UnauthorizedAccessException("Mã OTP đã hết hạn hoặc không tồn tại!");
        }

        // =========================================================================
        // CÁC HÀM KHÁC GIỮ NGUYÊN BÊN DƯỚI...
        // =========================================================================
        public async Task<object> GetAiKnowledgeBaseAsync()
        {
            var rawDiseases = await _context.Diseases
                .Include(d => d.DiseaseSymptoms)
                .ToListAsync();

            var aiKnowledge = rawDiseases.Select(d => new {
                diseaseName = d.DiseaseName,
                description = d.Description,
                treatment = d.TreatmentAdvice,
                weights = d.DiseaseSymptoms.ToDictionary(
                    ds => ds.SymptomId.ToString(),
                    ds => ds.WeightScore
                )
            }).ToList();

            return aiKnowledge;
        }

        public async Task<object> GetDiagnosisHistoryAsync(string username)
        {
            var sessions = await _context.DiagnosticSessions
                .AsSplitQuery()
                .Include(s => s.DiagnosisResults).ThenInclude(dr => dr.Disease)
                .Include(s => s.SessionSymptoms).ThenInclude(ss => ss.Symptom)
                .Where(s => s.User.Username == username)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return sessions.Select(s => {
                var topResult = s.DiagnosisResults.OrderByDescending(r => r.ProbabilityPercentage).FirstOrDefault();
                return new
                {
                    id = s.Id,
                    date = s.CreatedAt.ToString("dd/MM/yyyy, HH:mm"),
                    title = topResult?.Disease.DiseaseName ?? "Đang chờ phân tích",
                    accuracy = topResult != null ? $"{topResult.ProbabilityPercentage}%" : "--",
                    mainSymptoms = string.Join(", ", s.SessionSymptoms.Select(ss => ss.Symptom.SymptomName)),
                    status = "completed",
                    statusLabel = "Đã hoàn tất"
                };
            }).ToList();
        }

        public async Task<object> GetDiagnosisDetailAsync(long sessionId, string username)
        {
            var session = await _context.DiagnosticSessions
                .AsSplitQuery()
                .Include(s => s.DiagnosisResults).ThenInclude(dr => dr.Disease)
                .Include(s => s.SessionSymptoms).ThenInclude(ss => ss.Symptom)
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.User.Username == username);

            if (session == null) throw new Exception("Không tìm thấy kết quả hoặc bạn không có quyền xem.");

            var topResult = session.DiagnosisResults.OrderByDescending(r => r.ProbabilityPercentage).FirstOrDefault();
            var otherResults = session.DiagnosisResults.Where(r => topResult == null || r.Id != topResult.Id).ToList();

            return new
            {
                title = topResult?.Disease.DiseaseName ?? "Chưa có chẩn đoán",
                icdCode = topResult?.Disease.DiseaseCode ?? "N/A",
                accuracy = topResult != null ? $"{topResult.ProbabilityPercentage}%" : "0%",
                symptoms = session.SessionSymptoms.Select(ss => new {
                    title = ss.Symptom.SymptomName,
                    level = "Đã ghi nhận",
                    levelColor = "#3B82F6",
                    description = "Triệu chứng được người bệnh cung cấp"
                }),
                recommendations = new[] {
                    new { type = "activity", text = topResult?.Disease.TreatmentAdvice ?? "Nên theo dõi thêm và uống nhiều nước." },
                    new { type = "user", text = "Nên đến cơ sở y tế gần nhất nếu triệu chứng nặng hơn." }
                },
                differential = otherResults.Select(r => new {
                    name = r.Disease.DiseaseName,
                    value = $"{r.ProbabilityPercentage}%"
                })
            };
        }

        public async Task<bool> DeleteDiagnosisAsync(long sessionId, string username)
        {
            var session = await _context.DiagnosticSessions
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.User.Username == username);

            if (session == null) throw new Exception("Không tìm thấy kết quả hoặc bạn không có quyền xóa.");

            _context.DiagnosticSessions.Remove(session);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAllDiagnosisHistoryAsync(string username)
        {
            var sessions = await _context.DiagnosticSessions
                .Where(s => s.User.Username == username)
                .ToListAsync();

            if (sessions.Any())
            {
                _context.DiagnosticSessions.RemoveRange(sessions);
                await _context.SaveChangesAsync();
            }
            return true;
        }

        public async Task<bool> SubmitMobileFeedbackAsync(string username, long? sessionId, string comments)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) throw new Exception("Lỗi xác thực người dùng.");

            var feedback = new Feedback
            {
                UserId = user.Id,
                Comments = comments,
                CreatedAt = DateTime.Now
            };

            if (sessionId.HasValue)
            {
                var sessionExists = await _context.DiagnosticSessions
                    .AnyAsync(s => s.Id == sessionId.Value && s.User.Id == user.Id);

                if (sessionExists)
                {
                    feedback.SessionId = sessionId.Value;
                }
            }

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}