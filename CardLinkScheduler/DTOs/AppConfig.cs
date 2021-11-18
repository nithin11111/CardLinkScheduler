using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CardLinkScheduler.DTOs
{
    public class JwtInfo
    {
        public string Key { get; set; }
        public string Issuer { get; set; }
        public string Subject { get; set; }
        public string Audience { get; set; }
        public string ExpiryTime { get; set; }
        public string expiryTimeAdmin { get; set; }
        public string CardLinkBankId { get; set; }
    }
}
