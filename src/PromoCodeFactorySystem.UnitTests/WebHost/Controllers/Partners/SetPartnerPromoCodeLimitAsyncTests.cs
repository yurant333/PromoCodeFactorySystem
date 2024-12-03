using System;
using System.Collections.Generic;
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

    [SetUp]
    public void SetUp()
    {
        var fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        _partnersRepository = fixture.Freeze<IRepository<Partner>>();
        _partnersController = fixture.Build<PartnersController>().OmitAutoProperties().Create();
    }
    
    public Partner CreateBasePartner()
    {
        return new Partner
        {
            Id = Guid.Parse("7d994823-8226-4273-b063-1a95f3cc1df8"),
            Name = "Суперигрушки",
            IsActive = true,
            PartnerLimits = new List<PartnerPromoCodeLimit>
            {
                new PartnerPromoCodeLimit
                {
                    Id = Guid.Parse("e00633a5-978a-420e-a7d6-3e1dab116393"),
                    CreateDate = new DateTime(2020, 07, 9),
                    EndDate = new DateTime(2020, 10, 9),
                    Limit = 100
                }
            }
        };
    }
    
    [TestCase(1)]
    [TestCase(10000)]
    [TestCase(100)]
    [TestCase(1000)]
    public async Task SetPartnerPromoCodeLimitAsync_PartnerIsActive_ReturnsGoodRequest(int limit)
    {
        // Arrange
        var partnerId = Guid.Parse("def47943-7aaf-44a1-ae21-05aa4948b165");
        var partner = CreateBasePartner();
        partner.IsActive = true;
        var lastLimit = partner.PartnerLimits.FirstOrDefault(l=>l.CancelDate == null);
        var oldLimitIds = partner.PartnerLimits.Select(l => l.Id).ToArray();
        var limitEndDate = DateTime.Today;
        var request = new SetPartnerPromoCodeLimitRequest{ Limit = limit, EndDate = limitEndDate};

        _partnersRepository.GetByIdAsync(partnerId).Returns(Task.FromResult(partner));
        

        // Act
        var result = await _partnersController.SetPartnerPromoCodeLimitAsync(partnerId, request);
        

        // Assert
        result.Should().BeAssignableTo<CreatedAtActionResult>();
        
        var newLimit =   partner.PartnerLimits.FirstOrDefault(l =>! oldLimitIds.Contains(l.Id));
        newLimit.Should().NotBeNull();
        newLimit.EndDate.Should().Be(limitEndDate);
        newLimit.Limit.Should().Be(limit);
        
        lastLimit.Should().NotBeNull();
        lastLimit.CancelDate.Should().NotBeNull();
        partner.NumberIssuedPromoCodes.Should().Be(0);
    }
    
    [Test]
    public async Task SetPartnerPromoCodeLimitAsync_PartnerIsNotFound_ReturnsNotFound()
    {
        // Arrange
        var partnerId = Guid.Parse("def47943-7aaf-44a1-ae21-05aa4948b165");
        var partner = CreateBasePartner();
        partner.IsActive = true;
        var request = new SetPartnerPromoCodeLimitRequest{ Limit = 1};

        _partnersRepository.GetByIdAsync(partnerId).Returns(Task.FromResult(partner));
        

        // Act
        var result = await _partnersController.SetPartnerPromoCodeLimitAsync(Guid.NewGuid(), request);
        

        // Assert
        result.Should().BeAssignableTo<NotFoundResult>();
    }
    
    [Test]
    public async Task SetPartnerPromoCodeLimitAsync_PartnerIsNotActive_ReturnsBadRequest()
    {
        // Arrange
        var partnerId = Guid.Parse("def47943-7aaf-44a1-ae21-05aa4948b165");
        var partner = CreateBasePartner();
        partner.IsActive = false;
        var request = new SetPartnerPromoCodeLimitRequest{ Limit = 1};

        _partnersRepository.GetByIdAsync(partnerId).Returns(Task.FromResult(partner));
        

        // Act
        var result = await _partnersController.SetPartnerPromoCodeLimitAsync(partnerId, request);
        

        // Assert
        result.Should().BeAssignableTo<BadRequestObjectResult>();
    }
}