using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CardLinkScheduler.DTOs
{
    public sealed class OfferTransactionInitiationRequest
    {
        public Guid offer_id { get; set; }
        public Guid tenant_id { get; set; }
        public offerTransactiondetails offer_transaction_details { get; set; }
        public string status { get; set; }
        public string bankName { get; set; }
    }
    public sealed class offerTransactiondetails
    {
        public string discount_amount { get; set; }
        public string original_amount { get; set; }
        public string cheg_comission_amount { get; set; }
        public string merchantId { get; set; }
        public string Cheg_commission { get; set; }
        public string cheg_id { get; set; }
        public string bankuser_id { get; set; }
        public string ref_number { get; set; }
        public string terminal_id { get; set; }
        public string bank_local_date { get; set; }
        public string bank_local_time { get; set; }
        public string merchant_type { get; set; }
        public string merchant_name { get; set; }
    }
}


