using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CardLinkScheduler.DTOs
{
    /// <summary>
    /// Input request parameters for Error Logs. 
    /// </summary>

    public class RuleNameResponse
    {
        public string ruleField { get; set; }
        public List<BankTransaction> bankTransactionField { get; set; }
    }

    public class BankTransaction
    {
        public string name { get; set; }
    }
}
