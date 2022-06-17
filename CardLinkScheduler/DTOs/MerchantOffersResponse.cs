using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CardLinkScheduler.DTOs
{
    public sealed class AllCustomerInterest
    {
        public Guid tenant_id { get; set; }
        public Guid? id { get; set; }
        public OfferDetails offer_details { get; set; }
        public BusinessDetails business_details { get; set; }
        public List<string> merchant_id { get; set; }
        public string cheg_id { get; set; }
    }

    public class BusinessDetails
    {
        public string revenue { get; set; }
        public string commission { get; set; }
        public bool auto_approval { get; set; }
    }
    public class OfferDetails
    {
        public DiscountDetails discount_details { get; set; }
        public string offer_rule { get; set; }
    }
    public class DiscountDetails
    {
        public string discount_type { get; set; }
        //public long discount_type { get; set; }
        public string value { get; set; }
        public string min_order_amt { get; set; }
        public string max_discount_amt { get; set; }
    }

}


