using System;
using System.Threading.Tasks;

namespace PaymentsService.Src.Payments
{
    // Owner: Human Architect
    // Purpose: Domain entities and interfaces for the payment domain.
    // Tier: 3 (Critical)

    public class Charge
    {
        public string ChargeId { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal TotalRefunded { get; set; }
    }

    public class RefundCommand
    {
        public string ChargeId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public interface IBankApi
    {
        Task SendMoney(string customerId, decimal amount);
    }
}
