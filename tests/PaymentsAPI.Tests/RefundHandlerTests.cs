using System;
using System.Threading.Tasks;
using Moq;
using PaymentsService.Src.Payments;
using Xunit;

namespace PaymentsService.Tests.Payments
{
    // Owner: AI Agent (Verification Suite)
    // Purpose: Regression testing for PAY-4471 and edge case validation.
    // Tier: 3 (Critical)

    public class RefundHandlerTests
    {
        private readonly Mock<IBankApi> _mockBankApi;
        private readonly RefundHandler _handler;

        public RefundHandlerTests()
        {
            _mockBankApi = new Mock<IBankApi>();
            _handler = new RefundHandler(_mockBankApi.Object);
        }

        [Fact]
        public async Task HandleAsync_ValidPartialRefund_SucceedsAndUpdatesTotal()
        {
            // Arrange
            var charge = new Charge { ChargeId = "CH-100", CustomerId = "CUST-99", Amount = 250.00m, TotalRefunded = 0.00m };
            var cmd = new RefundCommand { ChargeId = "CH-100", Amount = 100.00m };

            // Act
            await _handler.HandleAsync(charge, cmd);

            // Assert
            Assert.Equal(100.00m, charge.TotalRefunded);
            _mockBankApi.Verify(x => x.SendMoney("CUST-99", 100.00m), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_RefundExceedsOriginalCharge_ThrowsInvalidOperationException()
        {
            // Arrange - PAY-4471 Reproduction Case ($400 refund attempt on a $250 charge)
            var charge = new Charge { ChargeId = "CH-100", CustomerId = "CUST-99", Amount = 250.00m, TotalRefunded = 0.00m };
            var cmd = new RefundCommand { ChargeId = "CH-100", Amount = 400.00m };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(charge, cmd));
            Assert.Contains("exceeds the remaining refundable balance", exception.Message);
            _mockBankApi.Verify(x => x.SendMoney(It.IsAny<string>(), It.IsAny<decimal>()), Times.Never);
        }

        [Fact]
        public async Task HandleAsync_SubsequentRefundExceedsRemaining_ThrowsInvalidOperationException()
        {
            // Arrange - Balance checks across multi-step mutations ($200 already refunded, attempt additional $60 on $250 total)
            var charge = new Charge { ChargeId = "CH-100", CustomerId = "CUST-99", Amount = 250.00m, TotalRefunded = 200.00m };
            var cmd = new RefundCommand { ChargeId = "CH-100", Amount = 60.00m };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(charge, cmd));
            _mockBankApi.Verify(x => x.SendMoney(It.IsAny<string>(), It.IsAny<decimal>()), Times.Never);
        }

        [Fact]
        public async Task HandleAsync_NegativeRefundAmount_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var charge = new Charge { ChargeId = "CH-100", CustomerId = "CUST-99", Amount = 250.00m, TotalRefunded = 0.00m };
            var cmd = new RefundCommand { ChargeId = "CH-100", Amount = -50.00m };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _handler.HandleAsync(charge, cmd));
            _mockBankApi.Verify(x => x.SendMoney(It.IsAny<string>(), It.IsAny<decimal>()), Times.Never);
        }

        [Fact]
        public async Task HandleAsync_ZeroRefundAmount_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var charge = new Charge { ChargeId = "CH-100", CustomerId = "CUST-99", Amount = 250.00m, TotalRefunded = 0.00m };
            var cmd = new RefundCommand { ChargeId = "CH-100", Amount = 0.00m };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _handler.HandleAsync(charge, cmd));
            _mockBankApi.Verify(x => x.SendMoney(It.IsAny<string>(), It.IsAny<decimal>()), Times.Never);
        }
    }
}
