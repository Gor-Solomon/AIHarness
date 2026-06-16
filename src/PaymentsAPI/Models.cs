using System;
using System.Threading.Tasks;

namespace PaymentsAPI.Models
{
    // Owner: Human Architect (Domain Models)
    // Purpose: Define the immutable state and contracts for the payment system.
    // Enforcement: Uses decimal for all monetary values as per .cursorrules.

    public class Charge
    {
        public Guid ChargeId { get; set; }
        public Guid CustomerId { get; set; }
        public decimal Amount { get; set; }
        public decimal TotalRefunded { get; set; }
    }

    public class RefundCommand
    {
        public Guid ChargeId { get; set; }
        public decimal Amount { get; set; }
    }

    public interface IBankApi
    {
        Task SendMoney(Guid customerId, decimal amount);
    }
}
