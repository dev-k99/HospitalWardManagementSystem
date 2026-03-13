using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WardSystemProject.Data;

namespace WardSystemProject.ViewComponents
{
    public class NewPrescriptionsBadgeViewComponent : ViewComponent
    {
        private readonly WardSystemDBContext _context;

        public NewPrescriptionsBadgeViewComponent(WardSystemDBContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (!User.IsInRole("Script Manager"))
                return Content(string.Empty);

            var count = await _context.Prescriptions
                .Where(p => p.IsActive && !_context.PrescriptionOrders.Any(po => po.PrescriptionId == p.Id))
                .CountAsync();

            return View(count);
        }
    }
}
