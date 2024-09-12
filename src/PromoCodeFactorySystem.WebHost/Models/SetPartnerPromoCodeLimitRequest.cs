using System;
using PromoCodeFactorySystem.Core.Domain.PromoCodeManagement;

namespace PromoCodeFactorySystem.WebHost.Models
{
    public class SetPartnerPromoCodeLimitRequest
    {
        public DateTime EndDate { get; set; }
        public int Limit { get; set; }
    }
}