using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CardLinkScheduler.Helpers
{
    public class AppSettings
    {
        public string ImportPath { get; set; }
        public string Outbox { get; set; }
        //public string FilteredPath { get; set; }
        public string CardLinkAPI { get; set; }
        public string IsProxy { get; set; }
        public string ArchivePath { get; set; }
        public string Inbox { get; set; }
        public string Localfile { get; set; }
        public string MerchantFileName { get; set; }
        public string TransactionFileName { get; set; }
        public string AllTransactionFileName { get; set; }
        public string BankName { get; set; }

        //public Redis Redis { get; set; }
    }

    public class BankSettings
    {
      //  public string Inbox { get; set; }
        public string BankIP { get; set; }
        public string BankUserName { get; set; }
        public string BankPassword { get; set; }
       // public string Outbox { get; set; }
    }

}
