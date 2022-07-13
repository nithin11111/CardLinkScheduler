using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CardLinkScheduler.DTOs
{
    public class TransactionDetailResponse
    {
        public TransactionDetail TransactionDetails { get; set; }
        public Guid TenantId { get; set; }
        public string BankName { get; set; }
    }

    public class TransactionDetail
    {
        public string DCARD { get; set; }
        public string AMOUNT { get; set; }
        public string REFNUM { get; set; }
        public string TERMID { get; set; }
        public string MASK_PAN { get; set; }
        public string LOCAL_DATE { get; set; }
        public string LOCAL_TIME { get; set; }
        public string Merchant_ID { get; set; }
        public string MERCHANT_TYPE { get; set; }
        public string Merchant_Name { get; set; }
        public string VirtualId { get; set; }
    }
}


