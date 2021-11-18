using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CardLinkScheduler.DTOs
{
    public sealed class OfferTransactionInitiationRequest
    {
        public DateTime Timestamp { get; set; }
        public offerTransactiondetails offer_trnasction_details { get; set; }
        public string amount { get; set; }
        public Guid OfferId { get; set; }
        public Guid TenantId { get; set; }
    }

    public sealed class offerTransactiondetails
    {
        public string amount { get; set; }
        public string currency { get; set; }
        public string description { get; set; }
    }
}


