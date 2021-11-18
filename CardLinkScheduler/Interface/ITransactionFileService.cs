using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CardLinkScheduler.Interface
{
    public interface ITransactionFileService
    {
        Task<string> UploadFileToCardlinkDB();
    }
}
