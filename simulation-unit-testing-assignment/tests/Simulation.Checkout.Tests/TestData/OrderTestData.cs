using Moq;
using Simulation.Checkout.Application;
using Simulation.Checkout.Application.Abstractions;
using AutoFixture ;
using Simulation.Checkout.Domain;
using Argon;
using FluentAssertions;
using Bogus;
using System.Runtime.CompilerServices;
using System.ComponentModel.Design;

public class TestData
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IInventoryService> _inventoryServiceMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    

    private readonly CheckoutService _checkoutService;

    public TestData()
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
public async Task CheckoutAsync_WhenOrderIsCreatedByAutoFixture_ShouldSucceed()
{
    // Arrange
    var fixture = new Fixture();

    fixture.Customize<OrderItem>(composer => composer
        .With(x => x.Quantity, 2)
        .With(x => x.UnitPrice, 100m)
    );

    var items = fixture.CreateMany<OrderItem>(2).ToList();

    var order = fixture.Build<Order>()
        .With(x => x.Items, items)
        .Create();

    // Act
    var receipt = await _checkoutService.CheckoutAsync(order);

    // Assert
    receipt.Should().NotBeNull();
    receipt.Total.Should().Be(400m);
}

[Fact]
public async Task CheckoutAsync_WhenCustomerIsGeneratedByBogus_ShouldReturnSameCustomerData()
    {
        // arrange
        var faker =  new Faker("en")
        {
            Random = new Randomizer(2026)
        };
        

        var customerName = faker.Name.FullName();
        var customerEmail = faker.Internet.Email(); 

        var fixture = new Fixture();

        var order =fixture.Build<Order>()
                    .With(order=>order.CustomerName,customerName)
                    .With(order=>order.CustomerEmail,customerEmail)
                    .With(order => order.Items, new List<OrderItem>
                        {
                            new OrderItem{
                               ProductId = Guid.NewGuid(),
                               ProductName = "Laptop",
                               UnitPrice = 100m,
                               Quantity = 2
                            }
                   })
                    .Create();

                    
       var recipt =await _checkoutService.CheckoutAsync(order);

       recipt.CustomerName.Should().Be(customerName);
       recipt.CustomerEmail.Should().Be(customerEmail);

        

    }



}