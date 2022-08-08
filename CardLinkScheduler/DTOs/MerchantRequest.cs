using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CardLinkScheduler.DTOs
{
    public class MerchantRequest
    {
        public List<string> merchant_id { get; set; }
        public List<string> merchantName { get; set; }
        public string fromDateTime { get; set; }
        public string toDateTime { get; set; }
        public List<string> accountNumber { get; set; }
        public List<string> virtualId { get; set; }
    }

    public sealed class MerchantListResponse
    {
        public string merchant_id { get; set; }
        public string merchant_name { get; set; }
    }
}
