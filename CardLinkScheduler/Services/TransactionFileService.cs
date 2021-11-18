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
    public class TransactionFileService : ITransactionFileService
    {
        private ITokenService _tokenService;
        private ICommonService _commonService;
        private readonly IOptions<AppSettings> _settings;
        private readonly IOptions<BankSettings> _bankSettings;

        public TransactionFileService(ITokenService TokenService, IOptions<AppSettings> settings, ICommonService commonService, IOptions<BankSettings> bankSettings)
        {
            _tokenService = TokenService;
            _settings = settings;
            _commonService = commonService;
            _bankSettings = bankSettings;
        }

        public async Task<string> UploadFileToCardlinkDB()
        {
            string result = string.Empty;
            try
            {
                string FilePath = await GetFilefromBank().ConfigureAwait(false);
                if (!string.IsNullOrEmpty(FilePath))
                {
                    result = FileUploadProcess(FilePath);
                }
            }
            catch (Exception ex)
            {
                ErrorLogRequest errorLogRequest = new ErrorLogRequest
                {
                    ErrorMsg = ex.Message,
                    FullMsg = ex.StackTrace,
                    MethodName = "UploadFileToCardlinkDB",
                    ErrorLineNo = new StackTrace(ex, true).GetFrame(0).GetFileLineNumber().ToString(),
                    ExceptionType = ex.GetType().ToString()
                };
                _commonService.InsertErrorLog(errorLogRequest);
            }
            return result;
        }

        public async Task<string> GetFilefromBank()
        {
            string localpath = string.Empty;
            try
            {
                WebClient client = new WebClient();
                string path = _settings.Value.Inbox;
                if (_settings.Value.IsProxy == "true")
                {
                    string username = _bankSettings.Value.BankUserName;
                    string pass = _bankSettings.Value.BankPassword;
                    NetworkCredential nc = new NetworkCredential(username, pass);
                    client.Credentials = nc;
                }
                //code to get list of files in remote directory
                DirectoryInfo info = new DirectoryInfo(path);
                FileInfo[] files = info.GetFiles("*.*", SearchOption.AllDirectories);
                foreach (var item in files.Where(t => t.Name == $"{_settings.Value.TransactionFileName}_{DateTime.Now.ToString("MM/dd/yyyy").Replace("-", "").Replace("/", "").Replace("/", "").Replace(":", "")}.csv"))
                {
                    string filename = item.ToString();
                    string serverpath = path + "\\" + Path.GetFileName(filename);// filename;
                    localpath = _settings.Value.Localfile + Path.GetFileName(filename);// filename;
                    await client.DownloadFileTaskAsync(new Uri(serverpath), localpath).ConfigureAwait(false);
                }
                //localpathContent = System.IO.File.ReadAllText(localpath);
                _commonService.MoveFile(_settings.Value.Inbox);
            }
            catch (Exception ex)
            {
                ErrorLogRequest errorLogRequest = new ErrorLogRequest
                {
                    ErrorMsg = ex.Message,
                    FullMsg = ex.StackTrace,
                    MethodName = "GetFilefromBank",
                    ErrorLineNo = new StackTrace(ex, true).GetFrame(0).GetFileLineNumber().ToString(),
                    ExceptionType = ex.GetType().ToString()
                };
                _commonService.InsertErrorLog(errorLogRequest);
            }
            return localpath;
        }

        public string FileUploadProcess(string filePath)
        {
            string result = string.Empty;
            try
            {
                var jwtaccesstoken = _tokenService.generateJwtTokenOnBankId();
                var client = new RestClient(_settings.Value.CardLinkAPI + "Upload/UploadFile");
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("Authorization", "Bearer " + jwtaccesstoken);
                request.AddHeader("Content-Type", "application/json");
                JObject jObjectbody = new JObject();
                jObjectbody.Add("filePath", filePath);
                jObjectbody.Add("fileId", "1");
                jObjectbody.Add("bankId", "BOM");
                string json = JsonConvert.SerializeObject(jObjectbody);
                request.AddParameter("application/json", json, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                result = response.Content;
                _commonService.MoveFile(filePath);
            }
            catch (Exception ex)
            {
                ErrorLogRequest errorLogRequest = new ErrorLogRequest
                {
                    ErrorMsg = ex.Message,
                    FullMsg = ex.StackTrace,
                    MethodName = "FileUploadProcess",
                    ErrorLineNo = new StackTrace(ex, true).GetFrame(0).GetFileLineNumber().ToString(),
                    ExceptionType = ex.GetType().ToString()
                };
                _commonService.InsertErrorLog(errorLogRequest);
            }
            return result;
        }
    }
}
