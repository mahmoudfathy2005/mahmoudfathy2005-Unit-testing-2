using Simulation.Checkout.Application;
using Simulation.Checkout.Infrastructure;
using Simulation.Checkout.Application.Abstractions;
using Simulation.Checkout.Domain;
using Xunit;
using FluentAssertions;
using NetArchTest;
using NetArchTest.Rules;

public class ArchitectureTests
{
     private readonly System.Reflection.Assembly _domainAssembly;
    private readonly System.Reflection.Assembly _applicationAssembly;
    private readonly System.Reflection.Assembly _infrastructureAssembly;

    public ArchitectureTests()
    {
        _domainAssembly=typeof(Order).Assembly;
        _infrastructureAssembly = typeof(CheckoutService).Assembly;
        _applicationAssembly = typeof(InMemoryOrderRepository).Assembly;
    }

    // Rule 5.1
    // Domain must not depend on Application or Infrastructure.

    [Fact]
    public void Domain_ShouldNotDependOnApplicationOrInfrastructure()
    {
       var result = Types.InAssembly(_domainAssembly)
            .Should()
            .NotHaveDependencyOnAny(
                "Simulation.Checkout.Application",
                "Simulation.Checkout.Infrastructure")
            .GetResult();

            result.IsSuccessful.Should().BeTrue();

    }

    // Rule 5.2
    // Application must not depend on Infrastructure.

    [Fact]
    public void Application_ShouldNotDependOnInfrastructure()
    {
        var result =Types.InAssembly(_applicationAssembly)
                        .Should()
                        .NotHaveDependencyOn("Simulation.Checkout.Infrastructure")
                        .GetResult();


           result.IsSuccessful.Should().BeTrue();
    }
    // Rule 5.3
    // All interfaces must start with I.

    [Fact]
    public void AllInterfaces_ShouldStartWithI()
    {
        
        

        var applicationResult= Types.InAssembly(_applicationAssembly)
                    .That()
                    .AreInterfaces()
                    .Should()
                    .HaveNameStartingWith("I")
                    .GetResult();

        // var infrastructureResult = Types.InAssembly(_infrastructureAssembly)
        //     .That()
        //     .AreInterfaces()
        //     .Should()
        //     .HaveNameStartingWith("I")
        //     .GetResult();

        // var domainResult =Types.InAssembly(_domainAssembly)
        // .That()
        // .AreInterfaces()
        // .Should()
        // .HaveNameStartingWith("I")
        // .GetResult();    


        applicationResult.IsSuccessful.Should().BeTrue();


        // domainResult.IsSuccessful.Should().BeTrue();


        // infrastructureResult.IsSuccessful.Should().BeTrue();
        

    }

    // Rule 5.4
    // Classes implementing IOrderRepository must end with Repository.

    [Fact]
    public void OrderRepositoryImplementations_ShouldEndWithRepository()
    {
        var result = Types.InAssembly(_infrastructureAssembly)
                .That()
                .ImplementInterface(typeof(IOrderRepository))// دي بتختار الـ Types اللي بتعمل Implement للـ Interface المحدد
                .Should()
                .HaveNameEndingWith("Repository")
                .GetResult()
                ;

                result.IsSuccessful.Should().BeTrue();
                

    }


}