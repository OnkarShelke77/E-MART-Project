using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EMart.Data;
using EMart.DTOs;
using EMart.Models;
using Microsoft.EntityFrameworkCore;

namespace EMart.Services
{
    // ===== Interface =====
    public interface IPaymentService
    {
        Task<PaymentResponseDto> CreatePaymentAsync(PaymentRequestDto dto);
        Task<List<PaymentResponseDto>> GetAllPaymentsAsync();
        Task<PaymentResponseDto> GetPaymentByIdAsync(int id);
        Task<List<PaymentResponseDto>> GetPaymentsByUserAsync(int userId);
    }

    // ===== Implementation =====
    public class PaymentService : IPaymentService
    {
        private readonly EMartDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IInvoicePdfService _invoicePdfService;

        public PaymentService(
            EMartDbContext context,
            IEmailService emailService,
            IInvoicePdfService invoicePdfService
        )
        {
            _context = context;
            _emailService = emailService;
            _invoicePdfService = invoicePdfService;
        }

        // @Transactional equivalent
        public async Task<PaymentResponseDto> CreatePaymentAsync(PaymentRequestDto dto)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var payment = new Payment
                {
                    OrderId = dto.OrderId,
                    UserId = dto.UserId,
                    AmountPaid = dto.AmountPaid,
                    PaymentMode = dto.PaymentMode,
                    PaymentStatus = !string.IsNullOrEmpty(dto.PaymentStatus)
                        ? dto.PaymentStatus
                        : "initiated",
                    TransactionId = dto.TransactionId,
                    PaymentDate = DateTime.UtcNow,
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                // Payment success → send mail + invoice
                if ("SUCCESS".Equals(payment.PaymentStatus, StringComparison.OrdinalIgnoreCase))
                {
                    var order =
                        await _context.Ordermasters.FirstOrDefaultAsync(o =>
                            o.Id == payment.OrderId
                        ) ?? throw new Exception("Order not found");

                    var items = await _context
                        .OrderItems.Where(oi => oi.OrderId == order.Id)
                        .ToListAsync();

                    byte[] invoicePdf = _invoicePdfService.GenerateInvoiceAsBytes(order, items);

                    try
                    {
                        await _emailService.SendPaymentSuccessMailAsync(order, invoicePdf);
                    }
                    catch (Exception ex)
                    {
                        // Email failure must not break payment flow
                        Console.WriteLine(ex.Message);
                    }
                }

                await transaction.CommitAsync();

                return await MapToDtoAsync(payment.Id);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<PaymentResponseDto>> GetAllPaymentsAsync()
        {
            var payments = await _context
                .Payments.Include(p => p.User)
                .Include(p => p.Order)
                .ToListAsync();

            return payments.Select(MapToDto).ToList();
        }

        public async Task<PaymentResponseDto> GetPaymentByIdAsync(int id)
        {
            var payment = await _context
                .Payments.Include(p => p.User)
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null)
                throw new Exception("Payment not found");

            return MapToDto(payment);
        }

        public async Task<List<PaymentResponseDto>> GetPaymentsByUserAsync(int userId)
        {
            var payments = await _context
                .Payments.Include(p => p.User)
                .Where(p => p.UserId == userId)
                .ToListAsync();

            return payments.Select(MapToDto).ToList();
        }

        // ===== Mapper =====
        private static PaymentResponseDto MapToDto(Payment p)
        {
            return new PaymentResponseDto
            {
                PaymentId = p.Id,
                AmountPaid = p.AmountPaid,
                PaymentMode = p.PaymentMode,
                PaymentStatus = p.PaymentStatus,
                TransactionId = p.TransactionId,
                PaymentDate = p.PaymentDate ?? DateTime.UtcNow,

                OrderId = p.OrderId,
                UserId = p.UserId,

                UserName = p.User?.FullName,
                UserEmail = p.User?.Email,
            };
        }

        private async Task<PaymentResponseDto> MapToDtoAsync(int paymentId)
        {
            var payment = await _context
                .Payments.Include(p => p.User)
                .Include(p => p.Order)
                .FirstAsync(p => p.Id == paymentId);

            return MapToDto(payment);
        }
    }
}
