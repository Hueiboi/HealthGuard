using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthGuard.Controllers
{
     [Authorize] // Tạm tắt nếu đang test cho tiện
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // Kiểm tra xem User đã đăng nhập chưa
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                ViewBag.UserName = User.Identity.Name;

                // GIẢ LẬP: Kiểm tra xem user này đã cập nhật hồ sơ chưa.
                // Thực tế sau này ông sẽ query: _context.HealthRecords.Any(x => x.UserId == id)
                // Tạm thời tui để "false" để ông test giao diện lúc chưa có data nhé.
                ViewBag.HasHealthRecords = false;
                ViewBag.HasRecentDiagnosis = true; // Tạm để true để test

                if (ViewBag.HasRecentDiagnosis)
                {
                    // Lấy ngày giờ thực tế và % từ DB truyền lên View
                    ViewBag.DiagnosisDate = DateTime.Now.ToString("dd MMM, yyyy");
                    ViewBag.DiagnosisScore = "87.5%"; // Sau này lấy % cao nhất từ cục JSON Python
                }
            }
            else
            {
                // Nếu là Guest thì chắc chắn chưa có hồ sơ
                ViewBag.HasHealthRecords = false;
                ViewBag.HasRecentDiagnosis = false;
            }

            return View();
        }
    }
}