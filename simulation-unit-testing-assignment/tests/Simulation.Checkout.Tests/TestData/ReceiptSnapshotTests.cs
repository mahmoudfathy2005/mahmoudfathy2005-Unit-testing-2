using FluentAssertions;
using Moq;
using Xunit;

using Simulation.Checkout.Application;
using Simulation.Checkout.Application.Abstractions;
using Simulation.Checkout.Domain;
using Simulation.Checkout.Domain.Exceptions;
using System.Net.Http.Headers;
using System.Data;
using VerifyTests;
namespace Simulation.Checkout.Tests;





public class ReceiptSnapshotTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IInventoryService> _inventoryServiceMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    

    private readonly CheckoutService _checkoutService;

    public ReceiptSnapshotTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _inventoryServiceMock = new Mock<IInventoryService>();
        _notificationServiceMock = new Mock<INotificationService>();

        _inventoryServiceMock
                .Setup(inventory => inventory
                .IsAvailableAsync(
                    It.IsAny<Guid>()
                    ,It.IsAny<int>()
                    ,It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

        _checkoutService = new CheckoutService(
            _orderRepositoryMock.Object,
            _inventoryServiceMock.Object,
            _notificationServiceMock.Object);

    }

    [Fact]
    public async Task CheckoutAsync_ExampleC_ShouldMatchSnapshot()
    {
        // Arrange 
        var order = new Order
    {
        CustomerName = "Ahmed",
        CustomerEmail = "ahmed@test.com",
        Items = new List<OrderItem>()
        {
            new OrderItem()
            {
              ProductId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
               ProductName = "Product B",
               UnitPrice =500m,
               Quantity =3
            }
        }
    };

    // Act 

    var recipt =await _checkoutService.CheckoutAsync(order);

    // arange 
    var setting = new VerifySettings(); //using VerifyTests;

    setting.IgnoreMember("OrderId");
    setting.IgnoreMember("CreatedAt");

    await Verifier.Verify(recipt,setting);

        
    }
}