using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CardLinkScheduler.DTOs
{
    public sealed class MerchantOffersResponse
    {
        public Guid Id { get; set; }
        public List<OfferDetails> OfferDetails { get; set; }
        public Guid TenantId { get; set; }
    }

    public class OfferDetails
    {
        public string merchant_id { get; set; }
        public string offer_type { get; set; }
        public string offer_rule { get; set; }
        public string offer_value { get; set; }
    }
}


