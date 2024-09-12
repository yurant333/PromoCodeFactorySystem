using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using PromoCodeFactorySystem.Core.Abstractions.Repositories;
using PromoCodeFactorySystem.Core.Domain.PromoCodeManagement;
using PromoCodeFactorySystem.WebHost.Controllers;
using NUnit.Framework;

namespace PromoCodeFactorySystem.UnitTests.WebHost.Controllers.Partners
{
    [TestFixture]
    public class CancelPartnerPromoCodeLimitAsyncTests
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

        [Test]
        public async Task CancelPartnerPromoCodeLimitAsync_PartnerIsNotFound_ReturnsNotFound()
        {
            // Arrange
            var partnerId = Guid.Parse("def47943-7aaf-44a1-ae21-05aa4948b165");
            Partner partner = null;

            _partnersRepository.GetByIdAsync(partnerId).Returns(Task.FromResult(partner));

            // Act
            var result = await _partnersController.CancelPartnerPromoCodeLimitAsync(partnerId);

            // Assert
            result.Should().BeAssignableTo<NotFoundResult>();
        }

        [Test]
        public async Task CancelPartnerPromoCodeLimitAsync_PartnerIsNotActive_ReturnsBadRequest()
        {
            // Arrange
            var partnerId = Guid.Parse("def47943-7aaf-44a1-ae21-05aa4948b165");
            var partner = CreateBasePartner();
            partner.IsActive = false;

            _partnersRepository.GetByIdAsync(partnerId).Returns(Task.FromResult(partner));

            // Act
            var result = await _partnersController.CancelPartnerPromoCodeLimitAsync(partnerId);

            // Assert
            result.Should().BeAssignableTo<BadRequestObjectResult>();
        }
    }
}
