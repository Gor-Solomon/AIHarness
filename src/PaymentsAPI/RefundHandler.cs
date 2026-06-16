using System;
using System.Threading.Tasks;

namespace PaymentsService.Src.Payments
{
    // Owner: AI Agent (Supervised by Senior Finance)
    // Purpose: Corrected refund logic to resolve PAY-4471.
    // Tier: 3 (Critical)

    public class RefundHandler
    {
        private readonly IBankApi _bankApi;

        public RefundHandler(IBankApi bankApi)
        {
            _bankApi = bankApi ?? throw new ArgumentNullException(nameof(bankApi));
        }

        public async Task HandleAsync(Charge charge, RefundCommand cmd)
        {
            if (charge == null)
            {
                throw new ArgumentNullException(nameof(charge));
            }

            if (cmd == null)
            {
                throw new ArgumentNullException(nameof(cmd));
            }

            // Calculation of remaining refundable limit
            decimal remainingRefundable = charge.Amount - charge.TotalRefunded;

            // Validation Path 1: Enforce boundaries against excessive financial leakage (PAY-4471 Bug Resolution)
            if (cmd.Amount > remainingRefundable)
            {
                throw new InvalidOperationException("Refund amount exceeds the remaining refundable balance of the original charge.");
            }

            // Validation Path 2: Guard system against negative value or zero-amount input vulnerabilities
            if (cmd.Amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cmd.Amount), "Refund amount must be strictly greater than zero.");
            }

            // Process external banking system transfer node
            await _bankApi.SendMoney(charge.CustomerId, cmd.Amount);

            // Mutate internal application entity tracking values safely
            charge.TotalRefunded += cmd.Amount;
        }
    }
}
