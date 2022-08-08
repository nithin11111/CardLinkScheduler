using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CardLinkScheduler.DTOs
{
    public class UploadFileRequest
    {
        public List<string> transactionResponse { get; set; }
        public string bankName { get; set; }
    }
}


