using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EMart.Services;
using EMart.Data;
using Microsoft.EntityFrameworkCore;

namespace EMart.Controllers
{
    [Authorize]
    [ApiController]
    [Route("invoice")] // Matches Java @RequestMapping("/invoice")
    public class InvoiceController : ControllerBase
    {
        private readonly EMartDbContext _context;
        private readonly IInvoicePdfService _invoicePdfService;

        public InvoiceController(EMartDbContext context, IInvoicePdfService invoicePdfService)
        {
            _context = context;
            _invoicePdfService = invoicePdfService;
        }

        [HttpGet("pdf/{orderId}")] // Matches Java /pdf/{orderId}
        public async Task<IActionResult> DownloadInvoice(int orderId)
        {
            var order = await _context.Ordermasters
                .Include(o => o.User)
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return NotFound("Order not found");

            var items = order.Items.ToList();
            var pdfBytes = _invoicePdfService.GenerateInvoicePdf(order, items);

            return File(pdfBytes, "application/pdf", $"invoice_{orderId}.pdf");
        }
    }
}
