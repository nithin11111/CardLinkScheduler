using CardLinkScheduler.DTOs;
using CardLinkScheduler.Interface;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CardLinkScheduler.Services
{
    public class MerchantFileCronJobService : CronJobService
    {
        private readonly ILogger<MerchantFileCronJobService> _logger;
        private IMerchantFileService _merchantFileService;
        private ICommonService _commonService;
        public MerchantFileCronJobService(IScheduleConfig<MerchantFileCronJobService> config, ILogger<MerchantFileCronJobService> logger, IMerchantFileService merchantFileService, ICommonService commonService)
            : base(config.CronExpression, config.TimeZoneInfo)
        {
            _logger = logger;
            _merchantFileService = merchantFileService;
            _commonService = commonService;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("CronJob 1 starts.");
            var objaudit = new AuditRequest
            {
                id = Guid.NewGuid(),
                timestamp = DateTime.UtcNow,
                tenant_id = Guid.Empty,
                audit_record = new AuditRecordRequest
                {
                    timestamp = DateTime.UtcNow,
                    action_performed = "POST",
                    correlation_id = Guid.NewGuid(),
                    user_performing_action = "StartAsync_Merchandise",
                    user_scope_used = "test"
                }
            };
            _commonService.InsertAuditLog(objaudit);
            _merchantFileService.SendMerchantFileToBank().Wait();
            return base.StartAsync(cancellationToken);
        }

        public override Task DoWork(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{DateTime.Now:hh:mm:ss} CronJob 1 is working.");
            var objaudit = new AuditRequest
            {
                id = Guid.NewGuid(),
                timestamp = DateTime.UtcNow,
                tenant_id = Guid.Empty,
                audit_record = new AuditRecordRequest
                {
                    timestamp = DateTime.UtcNow,
                    action_performed = "POST",
                    correlation_id = Guid.NewGuid(),
                    user_performing_action = "DoWork_Merchandise",
                    user_scope_used = "test"
                }
            };
            _commonService.InsertAuditLog(objaudit);
            //_merchantFileService.SendMerchantFileToBank().Wait();
            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("CronJob 1 is stopping.");
            return base.StopAsync(cancellationToken);
        }
    }
}
