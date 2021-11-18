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
    public class MerchantFileService : IMerchantFileService
    {
        private ITokenService _tokenService;
        private ICommonService _commonService;
        private readonly IOptions<AppSettings> _settings;
        private readonly IOptions<BankSettings> _bankSettings;

        public MerchantFileService(ITokenService TokenService, IOptions<AppSettings> settings, ICommonService commonService, IOptions<BankSettings> bankSettings)
        {
            _tokenService = TokenService;
            _settings = settings;
            _bankSettings = bankSettings;
            _commonService = commonService;
        }

        public async Task<bool> SendMerchantFileToBank()
        {
            bool success = false;
            try
            {
                string merchantList = GetUniqueMerchantList();
                string filePathName = "";
                if (!string.IsNullOrEmpty(merchantList))
                {
                    filePathName = await StoreMerchantList(merchantList).ConfigureAwait(false);
                }
                if (!string.IsNullOrEmpty(filePathName))
                {
                    success = await UploadMerchantFilesToBank(filePathName).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                ErrorLogRequest errorLogRequest = new ErrorLogRequest
                {
                    ErrorMsg = ex.Message,
                    FullMsg = ex.StackTrace,
                    MethodName = "SendMerchantFileToBank",
                    ErrorLineNo = new StackTrace(ex, true).GetFrame(0).GetFileLineNumber().ToString(),
                    ExceptionType = ex.GetType().ToString()
                };
                _commonService.InsertErrorLog(errorLogRequest);
            }
            return success;
        }

        public async Task<string> StoreMerchantList(string merchantList)
        {
            string filePathAndName = string.Empty;
            try
            {
                var fileName = $"{_settings.Value.MerchantFileName}_{DateTime.Now.ToString("MM/dd/yyyy").Replace("-", "").Replace("/", "").Replace("/", "").Replace(":", "")}.txt";
                filePathAndName = System.IO.Path.Combine(_settings.Value.Localfile, fileName);
                if (!Directory.Exists(filePathAndName))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(filePathAndName));
                }
                if (System.IO.File.Exists(filePathAndName))
                    System.IO.File.Delete(filePathAndName);
                using (var stream = new FileStream(
    filePathAndName, FileMode.Create, FileAccess.Write, FileShare.Write, 4096, useAsync: true))
                {
                    var bytes = Encoding.UTF8.GetBytes(merchantList);
                    await stream.WriteAsync(bytes, 0, bytes.Length);
                }
            }
            catch (Exception ex)
            {
                ErrorLogRequest errorLogRequest = new ErrorLogRequest
                {
                    ErrorMsg = ex.Message,
                    FullMsg = ex.StackTrace,
                    MethodName = "StoreMerchantList",
                    ErrorLineNo = new StackTrace(ex, true).GetFrame(0).GetFileLineNumber().ToString(),
                    ExceptionType = ex.GetType().ToString()
                };
                _commonService.InsertErrorLog(errorLogRequest);
            }
            return filePathAndName;
        }

        public string GetUniqueMerchantList()
        {
            string result = string.Empty;
            try
            {
                var jwtaccesstoken = _tokenService.generateJwtTokenOnBankId();
                var client = new RestClient(_settings.Value.CardLinkAPI + "Merchant/GetUniqueMerchantList");
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                request.AddHeader("Authorization", "Bearer " + jwtaccesstoken);
                request.AddHeader("Content-Type", "application/json");
                IRestResponse response = client.Execute(request);
                result = response.Content;
            }
            catch (Exception ex)
            {
                ErrorLogRequest errorLogRequest = new ErrorLogRequest
                {
                    ErrorMsg = ex.Message,
                    FullMsg = ex.StackTrace,
                    MethodName = "GetUniqueMerchantList",
                    ErrorLineNo = new StackTrace(ex, true).GetFrame(0).GetFileLineNumber().ToString(),
                    ExceptionType = ex.GetType().ToString()
                };
                _commonService.InsertErrorLog(errorLogRequest);
            }
            return result;
        }

        public async Task<bool> UploadMerchantFilesToBank(string filepath)
        {
            bool success = false;
            try
            {
                //code to upload files within network
                //string dns = "\\\\" + _bankSettings.Value.BankIP;
                //create a Share folder in Server in which you want to place files
                string Share = _settings.Value.Outbox;
                string serverpath = Share;
                WebClient client = new WebClient();
                string uri = serverpath + Path.GetFileName(filepath);
                Uri ServerPath1 = new Uri(uri);
                if (_settings.Value.IsProxy == "true")
                {
                    string username = _bankSettings.Value.BankUserName;
                    string pass = _bankSettings.Value.BankPassword;
                    NetworkCredential nc = new NetworkCredential(username, pass);
                    client.Credentials = nc;
                }
                byte[] arrReturn = await client.UploadFileTaskAsync(ServerPath1, filepath).ConfigureAwait(false);
                _commonService.MoveFile(Path.GetDirectoryName(filepath));
                success = true;
            }
            catch (Exception ex)
            {
                ErrorLogRequest errorLogRequest = new ErrorLogRequest
                {
                    ErrorMsg = ex.Message,
                    FullMsg = ex.StackTrace,
                    MethodName = "UploadMerchantFilesToBank",
                    ErrorLineNo = new StackTrace(ex, true).GetFrame(0).GetFileLineNumber().ToString(),
                    ExceptionType = ex.GetType().ToString()
                };
                _commonService.InsertErrorLog(errorLogRequest);
            }
            return success;
        }
    }
}
