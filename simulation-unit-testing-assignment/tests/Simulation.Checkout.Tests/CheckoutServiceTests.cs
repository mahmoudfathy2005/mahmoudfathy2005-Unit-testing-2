using FluentAssertions;
using Moq;
using Xunit;

using Simulation.Checkout.Application;
using Simulation.Checkout.Application.Abstractions;
using Simulation.Checkout.Domain;
using Simulation.Checkout.Domain.Exceptions;
using System.Net.Http.Headers;
namespace Simulation.Checkout.Tests;

public class CheckoutServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IInventoryService> _inventoryServiceMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly CheckoutService _checkoutService;

    public CheckoutServiceTests()
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
   public async Task CheckoutAsync_EmptyOrder_ShouldThrowException()
    {
        // Arrange
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerName = "John Doe",
            CustomerEmail = "john.doe@example.com",
            Items = new List<OrderItem>() // Empty order items
        };

        // Act
        Func<Task> act = async () =>await _checkoutService.CheckoutAsync(order);
       // assert
       await act.Should().ThrowAsync<EmptyOrderException>()
            .WithMessage("An order must contain at least one item.");

    //     // Act & Assert
    // await Assert.ThrowsAsync<EmptyOrderException>( () => _checkoutService.CheckoutAsync(order));


    }

[Theory]
[InlineData(0)]
[InlineData(-1)]
public async Task CheckoutAsync_InvalidQuantity_ShouldThrowInvalidQuantityException(int invalidQuantity)
{
    // Arrange  
    var order = new Order
    {
        Id = Guid.NewGuid(),
        CustomerName = "John Doe",
        CustomerEmail = "john.doe@example.com",
        Items = new List<OrderItem>
        {
            new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Laptop",
                UnitPrice = 1000m,
                Quantity = invalidQuantity
            }
        }
    };

     // Act
     var act = async () =>await _checkoutService.CheckoutAsync(order);
    await act.Should().ThrowAsync<InvalidQuantityException>()
        .WithMessage($"Quantity for 'Laptop' must be greater than zero.");

     // Act & Assert
    //  var ex = await Assert.ThrowsAsync<InvalidQuantityException>(() => _checkoutService.CheckoutAsync(order));

    //  Assert.Equal($"Quantity for 'Laptop' must be greater than zero.", ex.Message);

}


[Theory]
[InlineData(-1.00)]
[InlineData(-100.00)]
public async Task CheckoutAsync_InvalidPrice_ShouldThrowException_ShouldThrowInvalidPriceException(decimal invalidPrice)
{
    //Arrange 

    var order = new Order
    {
        Id = Guid.NewGuid(),
        CustomerName = "John Doe",
        CustomerEmail = "mahmoud@gmail.com",
        Items = new List<OrderItem>
        {
            new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Laptop",
                UnitPrice = invalidPrice,
                Quantity = 1
            }
        }
    };  

    // Act
    var act = async () => await _checkoutService.CheckoutAsync(order);

    // Assert
    await act.Should().ThrowAsync<InvalidPriceException>()
        .WithMessage($"Unit price for 'Laptop' cannot be negative.");

}

[Fact]
public async Task CheckoutAsync_InsufficientInventory_ShouldThrowException()
{
    // Arrange
    var order = new Order
    {
        Id = Guid.NewGuid(),
        CustomerName = "John Doe",
        CustomerEmail = "john.doe@example.com",
        Items = new List<OrderItem>
        {
            new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Laptop",
                UnitPrice = 1000m,
                Quantity = 5
            }
        }
    };
    // Mock the inventory service to return false for the inventory check
    _inventoryServiceMock
    .Setup(x => x.IsAvailableAsync(
        It.IsAny<Guid>(),
        It.IsAny<int>(),
        It.IsAny<CancellationToken>()))
    .ReturnsAsync(false);
    // act
        var act = async () => await _checkoutService.CheckoutAsync(order);

    // Assert
   await act.Should().ThrowAsync<InsufficientInventoryException>()
            .WithMessage("Insufficient inventory for 'Laptop'.");

 }

   [Fact]
   public async Task CheckoutAsync_WhenOrderHasMultipleItems_ShouldCalculateCorrectSubtotal()
    {
        

        // Arrange
    var order = new Order
    {
        CustomerName = "Ahmed",
        CustomerEmail = "ahmed@test.com",
        Items = new[]
        {
            new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Laptop",
                UnitPrice = 500m,
                Quantity = 2
            },
            new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Mouse",
                UnitPrice = 100m,
                Quantity = 3
            },
            new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Keyboard",
                UnitPrice = 200m,
                Quantity = 1
            }
        }
    };

    // Act 
    var recipt = await _checkoutService.CheckoutAsync(order);
    // Assert
    recipt.Subtotal.Should().Be(1500m);
  
    }


[Theory]
[InlineData(500)]
[InlineData(999)]
    public async Task CheckoutAsync_WhenSubtotalIsLessThan1000_ShouldApplyNoDiscount(decimal price)
    {
        // Arrange
         var order = new Order
      {   Id =new Guid(),
        CustomerName = "Ahmed",
        CustomerEmail = "ahmed@test.com",
        Items = new[]
        {
            new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Laptop",
                UnitPrice = price,
                Quantity = 1
            }
        }
      };

      // Act 
     var receipt =await _checkoutService.CheckoutAsync(order);
     // Assert
        receipt.Discount.Should().Be(0);

        

    }


  [Theory]
[InlineData(1000,900)]
[InlineData(1500,1400)]
    public async Task CheckoutAsync_WhenSubtotalIsGreaterthan1000orEquel_ShouldApplyDiscount(decimal price,decimal exepetedtotal)
    {
        // Arrange
         var order = new Order
      {   Id =new Guid(),
        CustomerName = "Ahmed",
        CustomerEmail = "ahmed@test.com",
        Items = new[]
        {
            new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Laptop",
                UnitPrice = price,
                Quantity = 1
            }
        }
      };

      // Act 
     var receipt =await _checkoutService.CheckoutAsync(order);
     // Assert
       receipt.Total.Should().Be(exepetedtotal);

        

    }

    [Fact]
public async Task CheckoutAsync_ShouldCreateReceiptWithCorrectLines()
{
    // Arrange
    var order = new Order
    {
        CustomerName = "Ahmed",
        CustomerEmail = "ahmed@test.com",
        Items = new[]
        {
            new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Laptop",
                UnitPrice = 500m,
                Quantity = 2
            },
            new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Mouse",
                UnitPrice = 100m,
                Quantity = 1
            }
        }
    };

    // Act
    var receipt = await _checkoutService.CheckoutAsync(order);

    // Assert
    receipt.Items.Should().HaveCount(2);

    receipt.Items.Should().ContainSingle(x =>
        x.ProductName == "Laptop" &&
        x.UnitPrice == 500m &&
        x.Quantity == 2);

    receipt.Items.Should().ContainSingle(x =>
        x.ProductName == "Mouse" &&
        x.UnitPrice == 100m &&
        x.Quantity == 1);
}

    [Fact]
public async Task CheckoutAsync_WhenCheckoutSucceeds_ShouldSaveOrder()
{
    // Arrange
    var order = new Order
    {
        CustomerName = "Ahmed",
        CustomerEmail = "ahmed@test.com",
        Items = new[]
        {
            new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Laptop",
                UnitPrice = 500m,
                Quantity = 1
            }
        }
    };

    // Act
    await _checkoutService.CheckoutAsync(order);

    // Assert
    _orderRepositoryMock.Verify(
        x => x.SaveAsync(
            It.IsAny<Order>(),
            It.IsAny<CancellationToken>()),
        Times.Once);
}

[Fact]
public async Task CheckoutAsync_WhenCheckoutSucceeds_ShouldSendOrderConfirmation()
{
    // Arrange
    var order = new Order
    {
        CustomerName = "Ahmed",
        CustomerEmail = "ahmed@test.com",
        Items = new[]
        {
            new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Laptop",
                UnitPrice = 500m,
                Quantity = 1
            }
        }
    };

    // Act
    await _checkoutService.CheckoutAsync(order);

    // Assert
    _notificationServiceMock.Verify(
        x => x.SendOrderConfirmationAsync(
            It.IsAny<CheckoutReceipt>(),
            It.IsAny<CancellationToken>()),
        Times.Once);
}


[Fact]
public async Task CheckoutAsync_WhenCheckoutFaild_ShouldNotSaveAndNOtSendConfirmarion()
    {
        // Arrange
        var order = new Order
    {
        CustomerName = "Ahmed",
        CustomerEmail = "ahmed@test.com",
        Items = new List<OrderItem>(),
        
    };
       // Act
        Func<Task> act = async () =>await _checkoutService.CheckoutAsync(order);
       // assert
       await act.Should().ThrowAsync<EmptyOrderException>()
            .WithMessage("An order must contain at least one item.");

      _orderRepositoryMock.Verify(x=>
         x.SaveAsync(
                      It.IsAny<Order>(),
                       It.IsAny<CancellationToken>()
                    ),
                    Times.Never
        );

        _notificationServiceMock.Verify( x=>
            x.SendOrderConfirmationAsync(
                It.IsAny<CheckoutReceipt>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Never

        );


        
    }


}