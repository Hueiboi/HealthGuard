using HealthGuard.Data;
using HealthGuard.Models.Dto;
using HealthGuard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic; // 🔥 THÊM ĐỂ KHÔNG BỊ LỖI LIST<LONG> KHI XÓA
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HealthGuard.Controllers
{
    [Authorize]
    public class DiagnosticController : Controller
    {
        private readonly DiagnosticService _diagnosticService;
        private readonly HealthContext _context;

        public DiagnosticController(DiagnosticService diagnosticService, HealthContext context)
        {
            _diagnosticService = diagnosticService;
            _context = context;
        }

        [HttpGet] public IActionResult Index() => View();
        [HttpGet] public IActionResult Review() => View();

        // 🔥 ĐÃ FIX: Hàm Details nhận tham số ID và lấy dữ liệu chi tiết của phiên chẩn đoán
        [HttpGet]
        public async Task<IActionResult> Details(long id)
        {
            string username = User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(username)) return RedirectToAction("Login", "Auth");

            string cleanUsername = username.Trim().ToLower();

            // Tìm đúng phiên chẩn đoán theo ID và thuộc về đúng User (Bảo mật)
            var session = await _context.DiagnosticSessions
                .Include(s => s.DiagnosisResults).ThenInclude(dr => dr.Disease)
                .FirstOrDefaultAsync(s => s.Id == id &&
                    (s.User.Username.ToLower() == cleanUsername || s.User.Email.ToLower() == cleanUsername));

            if (session == null)
                return NotFound("Không tìm thấy kết quả chẩn đoán này hoặc bạn không có quyền xem.");

            // Truyền dữ liệu sang View Details.cshtml
            return View(session);
        }

        [HttpGet]
        public async Task<IActionResult> History()
        {
            string username = User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(username)) return RedirectToAction("Login", "Auth");

            string cleanUsername = username.Trim().ToLower();

            var historyData = await _context.DiagnosticSessions
                .Include(s => s.DiagnosisResults).ThenInclude(dr => dr.Disease)
                .Where(s => s.User.Username.ToLower() == cleanUsername || s.User.Email.ToLower() == cleanUsername)
                .OrderByDescending(s => s.CreatedAt).ToListAsync();

            return View(historyData);
        }

        [HttpGet("api/patient/diagnose/symptoms")]
        public async Task<IActionResult> SelectSymptoms()
        {
            var symptoms = await _context.Symptoms.Select(s => new SymptomDto { Id = s.Id, SymptomName = s.SymptomName }).ToListAsync();
            return Ok(symptoms);
        }

        [HttpPost("api/patient/diagnose")]
        public async Task<IActionResult> RunDiagnosisAsync([FromBody] DiagnosticRequestDto request)
        {
            if (request?.SelectedSymptoms == null || !request.SelectedSymptoms.Any())
                return BadRequest(new { message = "Vui lòng cung cấp ít nhất một triệu chứng." });

            try
            {
                string username = User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Name);

                if (string.IsNullOrEmpty(username))
                    return StatusCode(401, new { message = "Phiên đăng nhập không hợp lệ hoặc đã hết hạn." });

                var result = await _diagnosticService.PerformDiagnosisAsync(username, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Không tìm thấy tài khoản"))
                {
                    return StatusCode(401, new { message = ex.Message });
                }

                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("api/patient/diagnose/delete-selected")]
        public async Task<IActionResult> DeleteSelectedHistory([FromBody] List<long> sessionIds)
        {
            try
            {
                string username = User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Name);
                if (string.IsNullOrEmpty(username)) return Unauthorized(new { message = "Vui lòng đăng nhập." });

                string cleanUsername = username.Trim().ToLower();

                var sessionsToDelete = await _context.DiagnosticSessions
                    .Where(s => sessionIds.Contains(s.Id) &&
                           (s.User.Username.ToLower() == cleanUsername || s.User.Email.ToLower() == cleanUsername))
                    .ToListAsync();

                if (sessionsToDelete.Any())
                {
                    _context.DiagnosticSessions.RemoveRange(sessionsToDelete);
                    await _context.SaveChangesAsync();
                }

                return Ok(new { success = true, deletedCount = sessionsToDelete.Count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("api/patient/diagnose/delete-all")]
        public async Task<IActionResult> DeleteAllHistory()
        {
            try
            {
                string username = User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Name);
                if (string.IsNullOrEmpty(username)) return Unauthorized(new { message = "Vui lòng đăng nhập." });

                string cleanUsername = username.Trim().ToLower();

                var allSessions = await _context.DiagnosticSessions
                    .Where(s => s.User.Username.ToLower() == cleanUsername || s.User.Email.ToLower() == cleanUsername)
                    .ToListAsync();

                if (allSessions.Any())
                {
                    _context.DiagnosticSessions.RemoveRange(allSessions);
                    await _context.SaveChangesAsync();
                }

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("api/patient/diagnose/toggle-save/{id}")]
        public async Task<IActionResult> ToggleSaveHistory(long id)
        {
            try
            {
                var session = await _context.DiagnosticSessions.FindAsync(id);
                if (session == null) return NotFound(new { message = "Không tìm thấy phiên chẩn đoán." });

                session.IsSaved = !session.IsSaved;
                await _context.SaveChangesAsync();

                return Ok(new { success = true, isSaved = session.IsSaved });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}