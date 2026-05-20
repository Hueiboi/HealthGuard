using HealthGuard.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace HealthGuard.Controllers
{
    [Authorize]
    public class HandbookController : Controller
    {
        private readonly HealthContext _context;

        public HandbookController(HealthContext context)
        {
            _context = context;
        }

        // Hiển thị danh sách toàn bộ danh mục bệnh lý trong CSDL
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Cẩm nang y khoa";

            var diseases = await _context.Diseases
                .OrderBy(d => d.DiseaseName)
                .ToListAsync();

            return View(diseases);
        }
    }
}