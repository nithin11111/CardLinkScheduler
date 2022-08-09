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
using System.Security.Cryptography;
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

        public async Task UploadFileToCardlinkDB()
        {
            List<string> result = new List<string>();
            try
            {
                var merchantList = await GetUniqueMerchantList();
                if (merchantList.Count > 0)
                {
                    MerchantRequest merchantRequests = new MerchantRequest
                    {
                        merchant_id = merchantList.Select(t => t.merchant_id).ToList(),
                        merchantName = merchantList.Select(t => t.merchant_name).ToList(),
                    };
                    result = await GetTransactionFile(merchantRequests);
                }
                if (result.Count() > 0)
                {
                    FileUploadProcess(result, _settings.Value.BankName);
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
        }

        //public async Task<List<string>> GetFilefromBank()
        //{
        //    List<string> localpathList = new List<string>();
        //    string localpath = null;
        //    int i = 0;
        //    try
        //    {
        //        WebClient client = new WebClient();
        //        string path = _settings.Value.Inbox;
        //        if (_settings.Value.IsProxy == "true")
        //        {
        //            string username = _bankSettings.Value.BankUserName;
        //            string pass = _bankSettings.Value.BankPassword;
        //            NetworkCredential nc = new NetworkCredential(username, pass);
        //            client.Credentials = nc;
        //        }
        //        //code to get list of files in remote directory
        //        DirectoryInfo info = new DirectoryInfo(path);
        //        FileInfo[] files = info.GetFiles("*.*", SearchOption.AllDirectories);
        //        foreach (var item in files.Where(t => t.Name == $"{_settings.Value.TransactionFileName}_{DateTime.Now.ToString("MM/dd/yyyy").Replace("-", "").Replace("/", "").Replace("/", "").Replace(":", "")}.csv" || t.Name == $"{_settings.Value.AllTransactionFileName}_{DateTime.Now.AddDays(-1).ToString("dd/MMM/yyyy")}.csv"))
        //        {
        //            string filename = item.ToString();
        //            string serverpath = path + "\\" + Path.GetFileName(filename);// filename;
        //            localpath = _settings.Value.Localfile + Path.GetFileName(filename);// filename;
        //            await client.DownloadFileTaskAsync(new Uri(serverpath), localpath).ConfigureAwait(false);
        //            i++;
        //            localpathList.Add(localpath);
        //        }
        //        //localpathContent = System.IO.File.ReadAllText(localpath);
        //        _commonService.MoveFile(_settings.Value.Inbox);
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLogRequest errorLogRequest = new ErrorLogRequest
        //        {
        //            ErrorMsg = ex.Message,
        //            FullMsg = ex.StackTrace,
        //            MethodName = "GetFilefromBank",
        //            ErrorLineNo = new StackTrace(ex, true).GetFrame(0).GetFileLineNumber().ToString(),
        //            ExceptionType = ex.GetType().ToString()
        //        };
        //        _commonService.InsertErrorLog(errorLogRequest);
        //    }
        //    return localpathList;
        //}

        //public string FileUploadProcess(string filePath, string bankName)
        //{
        //    string result = string.Empty;
        //    try
        //    {
        //        var jwtaccesstoken = _tokenService.generateJwtTokenOnBankId();
        //        var client = new RestClient(_settings.Value.CardLinkAPI + "Upload/UploadFile");
        //        client.Timeout = -1;
        //        var request = new RestRequest(Method.POST);
        //        request.AddHeader("Authorization", "Bearer " + jwtaccesstoken);
        //        request.AddHeader("Content-Type", "application/json");
        //        JObject jObjectbody = new JObject();
        //        jObjectbody.Add("filePath", filePath);
        //        jObjectbody.Add("fileId", "1");
        //        jObjectbody.Add("bankName", bankName);
        //        jObjectbody.Add("filterOption", filePath.Contains(_settings.Value.AllTransactionFileName) ? "all" : "");
        //        string json = JsonConvert.SerializeObject(jObjectbody);
        //        request.AddParameter("application/json", json, ParameterType.RequestBody);
        //        IRestResponse response = client.Execute(request);
        //        result = response.Content;
        //        _commonService.MoveFile(filePath);
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLogRequest errorLogRequest = new ErrorLogRequest
        //        {
        //            ErrorMsg = ex.Message,
        //            FullMsg = ex.StackTrace,
        //            MethodName = "FileUploadProcess",
        //            ErrorLineNo = new StackTrace(ex, true).GetFrame(0).GetFileLineNumber().ToString(),
        //            ExceptionType = ex.GetType().ToString()
        //        };
        //        _commonService.InsertErrorLog(errorLogRequest);
        //    }
        //    return result;
        //}

        public bool FileUploadProcess(List<string> transactionResponse, string bankName)
        {
            bool result = false;
            try
            {
                var jwtaccesstoken = _tokenService.generateJwtTokenOnBankId();
                var client = new RestClient(_settings.Value.CardLinkAPI + "Upload/UploadFile");
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("Authorization", "Bearer " + jwtaccesstoken);
                request.AddHeader("Content-Type", "application/json");
                UploadFileRequest uploadFileRequest = new UploadFileRequest
                {
                    bankName = bankName,
                    transactionResponse = transactionResponse
                };
                string json = JsonConvert.SerializeObject(uploadFileRequest);
                request.AddParameter("application/json", json, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                if (response.Content != null)
                {
                    result = true;
                }

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

        //public string getvirtualid()
        //{
        //    string result = string.Empty;
        //    try
        //    {
        //        var jwtaccesstoken = _tokenService.generateJwtTokenOnBankId();
        //        var client = new RestClient("https://localhost:44303/" + "ValidateMobile/ValidateMobileNo");
        //        client.Timeout = -1;
        //        var request = new RestRequest(Method.POST);
        //        request.AddHeader("Authorization", "Bearer " + jwtaccesstoken);
        //        request.AddHeader("Content-Type", "application/json");
        //        // JObject jObjectbody = new JObject();
        //        //jObjectbody.Add("filePath", filePath);
        //        //jObjectbody.Add("fileId", "1");
        //        //jObjectbody.Add("bankId", "BOM");
        //        IBBankRequest iBBankRequest = new IBBankRequest();
        //        string ibRequest = "{\"mobileNo\":\"" + "919423261763" + "\",\"flag\":\"" + "false" + "\",\"requestId\":\"" + "1423432" + "\",\"chksum\":\"" + "13456" + "\",\"reqTimestamp\":\"" + Convert.ToDateTime("2019-09-17").ToShortDateString() + "\"}";
        //        string requestStringEN = EncryptStringFromBytes(ibRequest, "bbcdefghijklmnoq", "bbcdefghijklmnoq");
        //        iBBankRequest.request = requestStringEN;
        //        string json = JsonConvert.SerializeObject(iBBankRequest);
        //        request.AddParameter("application/json", json, ParameterType.RequestBody);
        //        IRestResponse response = client.Execute(request);
        //        result = response.Content;
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLogRequest errorLogRequest = new ErrorLogRequest
        //        {
        //            ErrorMsg = ex.Message,
        //            FullMsg = ex.StackTrace,
        //            MethodName = "FileUploadProcess",
        //            ErrorLineNo = new StackTrace(ex, true).GetFrame(0).GetFileLineNumber().ToString(),
        //            ExceptionType = ex.GetType().ToString()
        //        };
        //        _commonService.InsertErrorLog(errorLogRequest);
        //    }
        //    return result;
        //}
        //public class IBBankRequest
        //{
        //    public string request { get; set; }
        //}
        //private static AesManaged CreateAes(string encryptionKey, string vectorkey)
        //{
        //    var aes = new AesManaged();
        //    aes.Key = System.Text.Encoding.UTF8.GetBytes(encryptionKey); //UTF8-Encoding
        //    aes.IV = System.Text.Encoding.UTF8.GetBytes(vectorkey);//UT8-Encoding
        //    return aes;
        //}

        //public static string EncryptStringFromBytes(string text, string encryptionKey, string vectorkey)
        //{
        //    using (AesManaged aes = CreateAes(encryptionKey, vectorkey))
        //    {
        //        ICryptoTransform encryptor = aes.CreateEncryptor();
        //        using (MemoryStream ms = new MemoryStream())
        //        {
        //            using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        //            {
        //                using (StreamWriter sw = new StreamWriter(cs))
        //                    sw.Write(text);
        //                return Convert.ToBase64String(ms.ToArray());
        //            }
        //        }
        //    }
        //}

        public async Task<List<string>> GetTransactionFile(MerchantRequest merchantRequests)
        {
            List<string> result = new List<string>();
            try
            {
                var client = new RestClient(_bankSettings.Value.BankAPiUrl + "Transaction/GetTransactionFile");
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                //request.AddHeader("Authorization", "Bearer " + jwtaccesstoken);
                request.AddHeader("Content-Type", "application/json");
                string json = JsonConvert.SerializeObject(merchantRequests);
                request.AddParameter("application/json", json, ParameterType.RequestBody);
                IRestResponse response = await client.ExecuteAsync(request);
                if (!string.IsNullOrEmpty(response.Content))
                {
                    result = JsonConvert.DeserializeObject<List<string>>(response.Content);
                }
                return result;
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
                return result;
            }
        }

        public async Task<List<MerchantListResponse>> GetUniqueMerchantList()
        {
            List<MerchantListResponse> result = new List<MerchantListResponse>();
            try
            {
                var jwtaccesstoken = _tokenService.generateJwtTokenOnBankId();
                var client = new RestClient(_settings.Value.CardLinkAPI + "Merchant/GetUniqueMerchantList");
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                request.AddHeader("Authorization", "Bearer " + jwtaccesstoken);
                request.AddHeader("Content-Type", "application/json");
                IRestResponse response = await client.ExecuteAsync(request);
                if (!string.IsNullOrEmpty(response.Content))
                {
                    result = JsonConvert.DeserializeObject<List<MerchantListResponse>>(response.Content);
                }


                return result;
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
                return result;
            }

        }
    }
}
