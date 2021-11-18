using CardLinkScheduler.DTOs;
using CardLinkScheduler.Interface;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CardLinkScheduler.Services
{
    public class ReconcilationCronJobService : CronJobService
    {
        private readonly ILogger<ReconcilationCronJobService> _logger;
        private IFileReconcilationService _fileReconcilationService;
       private ICommonService _commonService;
        public ReconcilationCronJobService(IScheduleConfig<ReconcilationCronJobService> config, ILogger<ReconcilationCronJobService> logger, ICommonService commonService, IFileReconcilationService fileReconcilationService)
            : base(config.CronExpression, config.TimeZoneInfo)
        {
            _logger = logger;
            _commonService = commonService;
           _fileReconcilationService = fileReconcilationService;
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
                    // session_id = filterContext.HttpContext.Session.Id,
                    timestamp = DateTime.UtcNow,
                    action_performed = "POST",
                    correlation_id = Guid.NewGuid(),
                    //payload_of_information = ,
                    user_performing_action = "StartAsync_Merchandise",
                    user_scope_used = "test"
                }
            };
            _commonService.InsertAuditLog(objaudit);
            _fileReconcilationService.ReconcilationProcess();
            //_cardLinkFileService.SendBankTransactionFileToCardLink().Wait();
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
                    // session_id = filterContext.HttpContext.Session.Id,
                    timestamp = DateTime.UtcNow,
                    action_performed = "POST",
                    correlation_id = Guid.NewGuid(),
                    //payload_of_information = ,
                    user_performing_action = "DoWork_Merchandise",
                    user_scope_used = "test"
                }
            };
            _commonService.InsertAuditLog(objaudit);
            _fileReconcilationService.ReconcilationProcess();
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
