using CardLinkScheduler.DTOs;
using CardLinkScheduler.Helpers;
using CardLinkScheduler.Interface;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CardLinkScheduler.Services
{
    public class CommonService : ICommonService
    {
        private ITokenService _tokenService;
        private readonly IOptions<AppSettings> _settings;

        public CommonService(ITokenService TokenService, IOptions<AppSettings> settings)

        {
            _tokenService = TokenService;
            _settings = settings;
        }
        public string InsertErrorLog(ErrorLogRequest errorLogRequest)
        {
            string result = string.Empty;
            try
            {
                var jwtaccesstoken = _tokenService.generateJwtTokenOnBankId();
                var client = new RestClient(_settings.Value.CardLinkAPI + "Log/InsertErrorLogs");
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("Authorization", "Bearer " + jwtaccesstoken);
                request.AddHeader("Content-Type", "application/json");
                string json = JsonConvert.SerializeObject(errorLogRequest);
                request.AddParameter("application/json", json, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                result = response.Content;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return result;
        }

        public string InsertAuditLog(AuditRequest auditRequest)
        {
            string result = string.Empty;
            try
            {
                var jwtaccesstoken = _tokenService.generateJwtTokenOnBankId();
                var client = new RestClient(_settings.Value.CardLinkAPI + "Log/InsertAuditLogs");
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("Authorization", "Bearer " + jwtaccesstoken);
                request.AddHeader("Content-Type", "application/json");
                string json = JsonConvert.SerializeObject(auditRequest);
                request.AddParameter("application/json", json, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                result = response.Content;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return result;
        }

        public void MoveFile(string FilePath)
        {
            string result = string.Empty;
            try
            {
                var jwtaccesstoken = _tokenService.generateJwtTokenOnBankId();
                var client = new RestClient(_settings.Value.CardLinkAPI + "Upload/MoveFile");
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                request.AddHeader("Authorization", "Bearer " + jwtaccesstoken);
                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("importPath", FilePath);
                IRestResponse response = client.Execute(request);
                result = response.Content;
            }
            catch (Exception ex)
            {
                ErrorLogRequest errorLogRequest = new ErrorLogRequest
                {
                    ErrorMsg = ex.Message,
                    FullMsg = ex.StackTrace,
                    MethodName = "DeleteFile",
                    ErrorLineNo = new StackTrace(ex, true).GetFrame(0).GetFileLineNumber().ToString(),
                    ExceptionType = ex.GetType().ToString()
                };
                InsertErrorLog(errorLogRequest);
            }
        }
        //public async Task<string> GetBankLogFiles()
        //{
        //    string localpath = string.Empty;
        //    try
        //    {
        //        WebClient client = new WebClient();
        //        //string username = _bankSettings.Value.BankUserName;
        //        //string pass = _bankSettings.Value.BankPassword;
        //        //string path = _bankSettings.Value.LogFilepath;
        //        //code to get list of files in remote directory
        //        DirectoryInfo info = new DirectoryInfo(path);
        //        FileInfo[] files = info.GetFiles("*.*", SearchOption.AllDirectories);
        //        foreach (var item in files.Where(t => t.Name == $"POS_Txn_{DateTime.Now.ToString("MM/dd/yyyy").Replace("-", "").Replace("/", "").Replace("/", "").Replace(":", "")}.csv"))
        //        {
        //            string filename = item.ToString();
        //            string serverpath = path + "\\" + Path.GetFileName(filename);// filename;
        //            localpath = _settings.Value.ExportPath + Path.GetFileName(filename);// filename;
        //            NetworkCredential nc = new NetworkCredential(username, pass);
        //            await client.DownloadFileTaskAsync(new Uri(serverpath), localpath).ConfigureAwait(false);
        //        }
        //        //localpathContent = System.IO.File.ReadAllText(localpath);
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLogRequest errorLogRequest = new ErrorLogRequest
        //        {
        //            ErrorMsg = ex.Message,
        //            FullMsg = ex.StackTrace,
        //            MethodName = "GetFilefromBank"
        //        };
        //        _commonService.InsertErrorLog(errorLogRequest);
        //    }
        //    return localpath;
        //}
    }
}
