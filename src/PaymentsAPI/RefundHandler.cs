using System;
using System.Threading.Tasks;
using PaymentsAPI.Models;

namespace PaymentsAPI
{
    // Owner: AI Agent (Fixing PAY-4471)
    // Purpose: Handles refund logic with strict validation to prevent refunds exceeding original charges.
    // Enforcement: Throws exceptions for invalid states, satisfying Tier 3 governance.

    public class RefundHandler
    {
        private readonly IBankApi _bankApi;

        public RefundHandler(IBankApi bankApi)
        {
            _bankApi = bankApi;
        }

        public async Task ProcessRefundAsync(Charge charge, RefundCommand command)
        {
            // Requirement: Zero or Negative amounts are strictly forbidden
            if (command.Amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(command.Amount), "Refund amount must BE greater than zero.");
            }

            // Calculation logic for PAY-4471: Determine exactly how much is left to refund
            decimal remainingRefundable = charge.Amount - charge.TotalRefunded;

            // Enforcement logic: Block refund if it exceeds the remaining balance
            if (command.Amount > remainingRefundable)
            {
                throw new InvalidOperationException(
                    $"Refund amount {command.Amount} exceeds remaining refundable balance of {remainingRefundable} for Charge {charge.ChargeId}.");
            }

            // External integration: Call the bank API
            await _bankApi.SendMoney(charge.CustomerId, command.Amount);

            // State update: Increment the total refunded amount
            charge.TotalRefunded += command.Amount;
        }
    }
}
