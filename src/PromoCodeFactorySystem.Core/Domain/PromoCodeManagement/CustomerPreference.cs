using System;

namespace PromoCodeFactorySystem.Core.Domain.PromoCodeManagement
{
    public class CustomerPreference
    {
        public Guid CustomerId { get; set; }
        public virtual Customer Customer { get; set; }

        public Guid PreferenceId { get; set; }
        public virtual Preference Preference { get; set; }
    }
}