using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CardLinkScheduler.DTOs
{
    public class AuditRequest
    {
        public Guid id { get; set; }
        public DateTime timestamp { get; set; }
        public Guid? tenant_id { get; set; }
        public AuditRecordRequest audit_record { get; set; }
    }

    public class AuditRecordRequest
    {
        public string session_id { get; set; }
        public DateTime timestamp { get; set; }
        public string action_performed { get; set; }
        public Guid correlation_id { get; set; }
        public string payload_of_information { get; set; }
        public string user_performing_action { get; set; }
        public string user_scope_used { get; set; }
    }

}
