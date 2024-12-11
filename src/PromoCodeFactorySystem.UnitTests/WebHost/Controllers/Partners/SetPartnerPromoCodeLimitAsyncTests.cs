using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NUnit.Framework;
using PromoCodeFactorySystem.Core.Abstractions.Repositories;
using PromoCodeFactorySystem.Core.Domain.PromoCodeManagement;
using PromoCodeFactorySystem.WebHost.Controllers;
using PromoCodeFactorySystem.WebHost.Models;

namespace PromoCodeFactorySystem.UnitTests.WebHost.Controllers.Partners;

[TestFixture]
public class SetPartnerPromoCodeLimitAsyncTests
{
    private IRepository<Partner> _partnersRepository;
    private PartnersController _partnersController;
    private PartnerBuilder _partnerBuilder;

    [SetUp]
    public void SetUp()
    {
        var fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        _partnersRepository = fixture.Freeze<IRepository<Partner>>();
        _partnersController = fixture.Build<PartnersController>().OmitAutoProperties().Create();
        _partnerBuilder = new PartnerBuilder(fixture);
    }

    [Test]
    public async Task SetPartnerPromoCodeLimitAsync_PartnerIsNotFound_ReturnsNotFound()
    {
        // Arrange
        var partnerId = Guid.Parse("def58054-7aaf-44a1-ae21-05aa4948b165");
        var setPartnerPromoCodeLimitRequest = new SetPartnerPromoCodeLimitRequest
        {
            Limit = 10
        };
        // Act
        var result = await _partnersController.SetPartnerPromoCodeLimitAsync(partnerId, setPartnerPromoCodeLimitRequest);

        // Assert
        result.Should().BeAssignableTo<NotFoundResult>();
    }
        
    [Test]
    public async Task SetPartnerPromoCodeLimitAsync_PartnerIsNotActive_ReturnsBadRequest()
    {
        // Arrange
        var partner = _partnerBuilder.WithInactive().Build();
        _partnersRepository.GetByIdAsync(partner.Id).Returns(Task.FromResult(partner));
            
        var setPartnerPromoCodeLimitRequest = new SetPartnerPromoCodeLimitRequest
        {
            Limit = 11
        };
        // Act
        var result = await _partnersController.SetPartnerPromoCodeLimitAsync(partner.Id, setPartnerPromoCodeLimitRequest);

        // Assert
        result.Should().BeAssignableTo<BadRequestObjectResult>();
    }
        
    [Test]
    public async Task SetPartnerPromoCodeLimitAsync_NegativeLimit_ReturnsBadRequest()
    {
        // Arrange
        var partner = _partnerBuilder.WithDefaultLimit().Build();
        _partnersRepository.GetByIdAsync(partner.Id).Returns(Task.FromResult(partner));
            
        var setPartnerPromoCodeLimitRequest = new SetPartnerPromoCodeLimitRequest
        {
            Limit = -1
        };
        // Act
        var result = await _partnersController.SetPartnerPromoCodeLimitAsync(partner.Id, setPartnerPromoCodeLimitRequest);

        // Assert
        result.Should().BeAssignableTo<BadRequestObjectResult>();
    }

    [Test]
    public async Task SetPartnerPromoCodeLimitAsync_WithExistingLimit_SavesChanges()
    {
        // Arrange
        var partner = _partnerBuilder
            .WithDefaultLimit()
            .WithIssuedPromoCodes(5)
            .Build();
        _partnersRepository.GetByIdAsync(partner.Id).Returns(Task.FromResult(partner));
        var currentLimit = partner.PartnerLimits.Single();
            
        var setPartnerPromoCodeLimitRequest = new SetPartnerPromoCodeLimitRequest
        {
            Limit = 3,
            EndDate = DateTime.Now.AddDays(5)
        };
        // Act
        var result = await _partnersController.SetPartnerPromoCodeLimitAsync(partner.Id, setPartnerPromoCodeLimitRequest);

        // Assert
        result.Should().BeAssignableTo<CreatedAtActionResult>();

        var argumentsAssertion = Arg.Is<Partner>(x => x.NumberIssuedPromoCodes == 0
                                                      && x.PartnerLimits.Count == 2
                                                      && x.PartnerLimits.Any(l =>
                                                          l.Limit == setPartnerPromoCodeLimitRequest.Limit));
        await _partnersRepository.Received(1).UpdateAsync(argumentsAssertion);

        currentLimit.CancelDate.Should().NotBeNull();
            
        var newLimit = partner.PartnerLimits.Single(l => l.Id != currentLimit.Id);
        newLimit.EndDate.Should().Be(setPartnerPromoCodeLimitRequest.EndDate);
        newLimit.Limit.Should().Be(setPartnerPromoCodeLimitRequest.Limit);
    }
    
    [Test]
    public async Task SetPartnerPromoCodeLimitAsync_WithoutExistingLimit_SavesChanges()
    {
        // Arrange
        var partner = _partnerBuilder
            .Build();
        _partnersRepository.GetByIdAsync(partner.Id).Returns(Task.FromResult(partner));
            
        var setPartnerPromoCodeLimitRequest = new SetPartnerPromoCodeLimitRequest
        {
            Limit = 3,
            EndDate = DateTime.Now.AddDays(5)
        };
        // Act
        var result = await _partnersController.SetPartnerPromoCodeLimitAsync(partner.Id, setPartnerPromoCodeLimitRequest);

        // Assert
        result.Should().BeAssignableTo<CreatedAtActionResult>();

        var argumentsAssertion = Arg.Is<Partner>(x => x.NumberIssuedPromoCodes == 0
                                          && x.PartnerLimits.Count == 1
                                          && x.PartnerLimits.Any(l =>
                                              l.Limit == setPartnerPromoCodeLimitRequest.Limit));
        await _partnersRepository.Received(1).UpdateAsync(argumentsAssertion);
        
        var newLimit = partner.PartnerLimits.Single();
        newLimit.EndDate.Should().Be(setPartnerPromoCodeLimitRequest.EndDate);
        newLimit.Limit.Should().Be(setPartnerPromoCodeLimitRequest.Limit);
    }
}