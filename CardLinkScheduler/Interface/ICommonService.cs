using CardLinkScheduler.DTOs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CardLinkScheduler.Interface
{
    public interface ICommonService
    {
        string InsertErrorLog(ErrorLogRequest errorLogRequest);
        string InsertAuditLog(AuditRequest auditRequest);
        void MoveFile(string FilePath);
    }
}
