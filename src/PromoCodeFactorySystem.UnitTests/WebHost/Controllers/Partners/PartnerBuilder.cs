using System;
using System.Collections.Generic;
using AutoFixture;
using AutoFixture.Dsl;
using PromoCodeFactorySystem.Core.Domain.PromoCodeManagement;

namespace PromoCodeFactorySystem.UnitTests.WebHost.Controllers.Partners;

public class PartnerBuilder(IFixture fixture)
{
    private bool _isActive = true;
    private readonly ICustomizationComposer<Partner> _partnerCustomization = fixture.Build<Partner>();

    private readonly List<PartnerPromoCodeLimit> _partnerLimits = [];
    private int _numberIssuedPromoCodes;

    public PartnerBuilder WithIssuedPromoCodes(int numberIssuedPromoCodes)
    {
        _numberIssuedPromoCodes = numberIssuedPromoCodes;
        return this;
    }

    public PartnerBuilder WithInactive()
    {
        _isActive = false;
        return this;
    }

    public PartnerBuilder WithDefaultLimit()
    {
        var newLimit = new PartnerPromoCodeLimit()
        {
            Limit = 1,
            CreateDate = DateTime.Now.AddDays(-1),
            EndDate = DateTime.Now.AddDays(1),
            CancelDate = null,
            Id = Guid.NewGuid()
        };
        _partnerLimits.Add(newLimit);
        return this;
    }

    public Partner Build()
    {
        return _partnerCustomization.OmitAutoProperties()
            .With(partner => partner.IsActive, _isActive)
            .With(partner => partner.NumberIssuedPromoCodes, _numberIssuedPromoCodes)
            .With(partner => partner.PartnerLimits, _partnerLimits)
            .Create();
    }
}