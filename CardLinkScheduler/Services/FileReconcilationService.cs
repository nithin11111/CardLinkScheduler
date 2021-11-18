using CardLinkScheduler.DTOs;
using CardLinkScheduler.Helpers;
using CardLinkScheduler.Interface;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;

namespace CardLinkScheduler.Services
{
    public class FileReconcilationService : IFileReconcilationService
    {
        private ITokenService _tokenService;
        private ICommonService _commonService;
        private readonly IOptions<AppSettings> _settings;
        private readonly IOptions<BankSettings> _bankSettings;

        public FileReconcilationService(ITokenService TokenService, IOptions<AppSettings> settings, ICommonService commonService, IOptions<BankSettings> bankSettings)
        {
            _tokenService = TokenService;
            _settings = settings;
            _bankSettings = bankSettings;
            _commonService = commonService;
        }

        public void ReconcilationProcess()
        {
            //ApplyRule();
            IEnumerable<TransactionDetailResponse> transactionDetailResponse = Enumerable.Empty<TransactionDetailResponse>();
            string merchantOffers = GetMerchantOffers();
            // var interestData = GetCustomerInterest();
            var transationFile = JsonConvert.DeserializeObject<List<TransactionDetailResponse>>(GetTransactionFileFromCardLink());
            var rulesList = JsonConvert.DeserializeObject<List<RuleNameResponse>>(GetRuleNames("1"));
            var merchantOffersResponseList = JsonConvert.DeserializeObject<List<MerchantOffersResponse>>(merchantOffers);
            var merchantoffers = merchantOffersResponseList.SelectMany(t => t.OfferDetails).ToList();
            List<OfferTransactionInitiationRequest> transactionList = new List<OfferTransactionInitiationRequest>();
            foreach (var item in transationFile)
            {
                if (merchantoffers.Where(t=>t.merchant_id == item.TransactionDetails.Merchant_ID.TrimEnd()).Count() > 0)
                {
                    var merchantOfferDetail = merchantoffers.Where(t => t.merchant_id == item.TransactionDetails.Merchant_ID.TrimEnd()).FirstOrDefault();
                    var merchantofferrules = "(;merchant;==;Amazon;||;(;(;location;IN;bangalore,delhi,mangalore;);||;(;billing Amount;==;16;&&;gender;!=;female;);););&&;age;>;16;&&;(;(;card;==;master;&&;(;monthly amount limit;>=;15;&&;monthly amount limit;<=;20;););||;onlynewuser;==;true;)";
                   // var merchantofferrules = merchantOfferDetail.offer_rule;
                    if (merchantofferrules != null)
                    {
                        if (merchantofferrules.ToLower().Contains("merchant") && rulesList.Where(t => t.ruleField.ToLower() == "merchant" && t.bankTransactionField.Where(t => t.name.ToLower() == "vendor").Count() > 0).Count() > 0)
                        {
                            //ToDO:transaction value hardcoded
                            merchantofferrules = merchantofferrules.ToLower().Replace("merchant", "Amazon".ToLower());
                        }
                        if (merchantofferrules.ToLower().Contains("age") && rulesList.Where(t => t.ruleField.ToLower() == "age" && t.bankTransactionField.Where(t => t.name.ToLower() == "age").Count() > 0).Count() > 0)
                        {
                            //ToDO:transaction value hardcoded
                            merchantofferrules = merchantofferrules.ToLower().Replace("age", "17");
                        }
                        if (merchantofferrules.ToLower().Contains("location") && rulesList.Where(t => t.ruleField.ToLower() == "location" && t.bankTransactionField.Where(t => t.name.ToLower() == "place").Count() > 0).Count() > 0)
                        {
                            //ToDO:transaction value hardcoded
                            merchantofferrules = merchantofferrules.ToLower().Replace("location", "mangalore".ToLower());
                        }
                        if (merchantofferrules.ToLower().Contains("gender") && rulesList.Where(t => t.ruleField.ToLower() == "gender" && t.bankTransactionField.Where(t => t.name.ToLower() == "gender").Count() > 0).Count() > 0)
                        {
                            //ToDO:transaction value hardcoded
                            merchantofferrules = merchantofferrules.ToLower().Replace("gender", "male".ToLower());
                        }
                        if (merchantofferrules.ToLower().Contains("billing amount") && rulesList.Where(t => t.ruleField.ToLower() == "billing amount" && t.bankTransactionField.Where(t => t.name.ToLower() == "billing amt").Count() > 0).Count() > 0)
                        {
                            //ToDO:transaction value hardcoded
                            merchantofferrules = merchantofferrules.ToLower().Replace("billing amount", "15");
                        }
                        if (merchantofferrules.ToLower().Contains("bank") && rulesList.Where(t => t.ruleField.ToLower() == "bank" && t.bankTransactionField.Where(t => t.name.ToLower() == "bank").Count() > 0).Count() > 0)
                        {
                            //ToDO:transaction value hardcoded
                            merchantofferrules = merchantofferrules.ToLower().Replace("bank", "BOM".ToLower());
                        }
                        if (merchantofferrules.ToLower().Contains("card") && rulesList.Where(t => t.ruleField.ToLower() == "card" && t.bankTransactionField.Where(t => t.name.ToLower() == "card").Count() > 0).Count() > 0)
                        {
                            //ToDO:transaction value hardcoded
                            merchantofferrules = merchantofferrules.ToLower().Replace("card", "Master".ToLower());
                        }
                        if (merchantofferrules.ToLower().Contains("max discount amt") && rulesList.Where(t => t.ruleField.ToLower() == "max discount amt" && t.bankTransactionField.Where(t => t.name.ToLower() == "max discount amt").Count() > 0).Count() > 0)
                        {
                            //ToDO:transaction value hardcoded
                            merchantofferrules = merchantofferrules.ToLower().Replace("max discount amt", "14".ToLower());
                        }
                        if (merchantofferrules.ToLower().Contains("monthly amount limit") && rulesList.Where(t => t.ruleField.ToLower() == "monthly amount limit" && t.bankTransactionField.Where(t => t.name.ToLower() == "monthly amount limit").Count() > 0).Count() > 0)
                        {
                            //ToDO:transaction value hardcoded
                            merchantofferrules = merchantofferrules.ToLower().Replace("monthly amount limit", "16".ToLower());
                        }
                        if (merchantofferrules.ToLower().Contains("monthly transaction limit") && rulesList.Where(t => t.ruleField.ToLower() == "monthly transaction limit" && t.bankTransactionField.Where(t => t.name.ToLower() == "monthly transaction limit").Count() > 0).Count() > 0)
                        {
                            //ToDO:transaction value hardcoded
                            merchantofferrules = merchantofferrules.ToLower().Replace("monthly transaction limit", "16".ToLower());
                        }
                        if (merchantofferrules.ToLower().Contains("onlynewuser") && rulesList.Where(t => t.ruleField.ToLower() == "onlynewuser" && t.bankTransactionField.Where(t => t.name.ToLower() == "onlynewuser").Count() > 0).Count() > 0)
                        {
                            //ToDO:transaction value hardcoded
                            merchantofferrules = merchantofferrules.ToLower().Replace("onlynewuser", "true".ToLower());
                        }

                        string po = InfixToPostfix(merchantofferrules);

                        bool offerValid = ValidateRules(po);
                        if (offerValid)
                        {
                            if (merchantOfferDetail.offer_type == "percentage")
                            {
                                //ToDO: discount value hardcoded
                                item.TransactionDetails.AMOUNT = (Convert.ToDecimal(item.TransactionDetails.AMOUNT) % Convert.ToDecimal(merchantOfferDetail.offer_value)).ToString();
                            }
                            else
                            {
                                //ToDO: discount value hardcoded
                                item.TransactionDetails.AMOUNT = (Convert.ToDecimal(item.TransactionDetails.AMOUNT) > 15 ? Convert.ToDecimal(item.TransactionDetails.AMOUNT) - Convert.ToDecimal(merchantOfferDetail.offer_value) : 0).ToString();
                            }
                            OfferTransactionInitiationRequest otir = new OfferTransactionInitiationRequest
                            {
                                OfferId = merchantOffersResponseList.Where(t => t.OfferDetails.Where(d => d.merchant_id == item.TransactionDetails.Merchant_ID.TrimEnd()).Count() > 0).FirstOrDefault().Id,
                                amount = item.TransactionDetails.AMOUNT,
                                offer_trnasction_details = new offerTransactiondetails { currency = "rs", description = "max disc" },
                                TenantId = item.TenantId,
                                Timestamp = DateTime.Now
                            };
                            transactionList.Add(otir);
                        }
                    }
                }
            }
            SaveTransactionInitiation(transactionList);
        }
        public string GetCustomerInterest()
        {
            string result = string.Empty;
            try
            {
                var jwtaccesstoken = _tokenService.generateJwtTokenOnBankId();
                var client = new RestClient(_settings.Value.CardLinkAPI + "Merchant/GetCustomerInterest");
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                request.AddHeader("Authorization", "Bearer " + jwtaccesstoken);
                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("tenantId", "3ce2c869-3bce-4c19-b450-866142580760");
                IRestResponse response = client.Execute(request);
                result = response.Content;
            }
            catch (Exception ex)
            {
                ErrorLogRequest errorLogRequest = new ErrorLogRequest
                {
                    ErrorMsg = ex.Message,
                    FullMsg = ex.StackTrace,
                    MethodName = "GetCustomerInterest",
                    ErrorLineNo = new StackTrace(ex, true).GetFrame(0).GetFileLineNumber().ToString(),
                    ExceptionType = ex.GetType().ToString()
                };
                _commonService.InsertErrorLog(errorLogRequest);
            }
            return result;
        }

        public string GetRuleNames(string bankId)
        {
            string result = string.Empty;
            try
            {
                var jwtaccesstoken = _tokenService.generateJwtTokenOnBankId();
                var client = new RestClient(_settings.Value.CardLinkAPI + "Merchant/GetRuleNames");
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                request.AddHeader("Authorization", "Bearer " + jwtaccesstoken);
                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("bankId", bankId);
                IRestResponse response = client.Execute(request);
                result = response.Content;
            }
            catch (Exception ex)
            {
                ErrorLogRequest errorLogRequest = new ErrorLogRequest
                {
                    ErrorMsg = ex.Message,
                    FullMsg = ex.StackTrace,
                    MethodName = "GetRuleNames",
                    ErrorLineNo = new StackTrace(ex, true).GetFrame(0).GetFileLineNumber().ToString(),
                    ExceptionType = ex.GetType().ToString()
                };
                _commonService.InsertErrorLog(errorLogRequest);
            }
            return result;
        }

        public string GetMerchantOffers()
        {
            string result = string.Empty;
            try
            {
                var jwtaccesstoken = _tokenService.generateJwtTokenOnBankId();
                var client = new RestClient(_settings.Value.CardLinkAPI + "Merchant/MerchantOffers");
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("Authorization", "Bearer " + jwtaccesstoken);
                request.AddHeader("Content-Type", "application/json");
                JObject jObjectbody = new JObject();
                jObjectbody.Add("MerchantId", "0");
                string json = JsonConvert.SerializeObject(jObjectbody);
                request.AddParameter("application/json", json, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                result = response.Content;
            }
            catch (Exception ex)
            {
                ErrorLogRequest errorLogRequest = new ErrorLogRequest
                {
                    ErrorMsg = ex.Message,
                    FullMsg = ex.StackTrace,
                    MethodName = "GetMerchantOffers",
                    ErrorLineNo = new StackTrace(ex, true).GetFrame(0).GetFileLineNumber().ToString(),
                    ExceptionType = ex.GetType().ToString()
                };
                _commonService.InsertErrorLog(errorLogRequest);
            }
            return result;
        }

        public string GetTransactionFileFromCardLink()
        {
            string result = string.Empty;
            try
            {
                var jwtaccesstoken = _tokenService.generateJwtTokenOnBankId();
                var client = new RestClient(_settings.Value.CardLinkAPI + "Reconcilation/GetTransactionFile");
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
                    MethodName = "GetTransactionFileFromCardLink",
                    ErrorLineNo = new StackTrace(ex, true).GetFrame(0).GetFileLineNumber().ToString(),
                    ExceptionType = ex.GetType().ToString()
                };
                _commonService.InsertErrorLog(errorLogRequest);
            }
            return result;
        }

    //    public void ApplyRule()
    //    {
    //        try
    //        {
    //            string xml = "<rule>"
    //    //   "<allowedmerchant>"
    //    + "<Merchant>"
    //   + "<Name>amazon </Name>"
    //  + "<Operand> Equals </Operand>"
    //  + "</Merchant>"
    //      + "<Merchant>"
    //  + "<Name>amazon</Name>"
    // + "<Operand>NotEquals</Operand>"
    // + "</Merchant>"
    ////+ "</allowedmerchant>"
    //+ "</rule>";
    //            XmlDocument xmlDoc = new XmlDocument();
    //            xmlDoc.LoadXml(xml);
    //            string Json = JsonConvert.SerializeXmlNode(xmlDoc);
    //            var users = JObject.Parse(Json).SelectToken("rule").ToString();
    //            var rulesRequest = JsonConvert.DeserializeObject<Rules>(users);
                

    //            //RulesRequest rulesRequest = new RulesRequest
    //            //{
    //            //    AllowedMerchant = Json.
    //            //    {
    //            //        MerchantName = "test",
    //            //        Operand = "test"
    //            //    }.ToList();
    //            //};
    //            //if(xmlDoc.GetElementsByTagName("rule").Count > 1)
    //            //{
    //            //    XmlNodeList xmlnode = xmlDoc.GetElementsByTagName("rule");
    //            //    for (int i = 0; i <= xmlnode.Count - 1; i++)
    //            //    {
    //            //       if(xmlnode[i].ChildNodes.Item(0).Name == "allowedmerchant")
    //            //        {

    //            //        }
    //            //    }

    //            //    }
    //            //XmlNodeList xmlNodeList = xmlDoc.SelectNodes("/info/collage");

    //        }
    //        catch(Exception ex)
    //        { }
    //    }

        public bool ValidateRules(string s)
        {
            int a, b;
            bool ans = false;
            Stack i = new Stack();
            string[] expression = s.Split(';');
            for (int j = 0; j < expression.Length; j++)
            {
                string c = expression[j];////po.Substring(j, 1)
                if (c.Equals("*"))
                {
                    string sa = Convert.ToString(i.Pop());
                    string sb = Convert.ToString(i.Pop());
                    a = Convert.ToInt32(sb);
                    b = Convert.ToInt32(sa);
                    i.Push(a * b);

                }
                else if (c.Equals("/"))
                {
                    string sa = Convert.ToString(i.Pop());
                    string sb = Convert.ToString(i.Pop());
                    a = Convert.ToInt32(sb);
                    b = Convert.ToInt32(sa);
                    i.Push(a / b);
                }
                else if (c.Equals("+"))
                {
                    string sa = Convert.ToString(i.Pop());
                    string sb = Convert.ToString(i.Pop());
                    a = Convert.ToInt32(sb);
                    b = Convert.ToInt32(sa);
                    i.Push(a + b);

                }
                else if (c.Equals("-"))
                {
                    string sa = Convert.ToString(i.Pop());
                    string sb = Convert.ToString(i.Pop());
                    a = Convert.ToInt32(sb);
                    b = Convert.ToInt32(sa);
                    i.Push(a - b);

                }
                else if (c.Equals("&&"))
                {
                    string sa =Convert.ToString(i.Pop());
                    string sb = Convert.ToString(i.Pop());
                    ans = Convert.ToBoolean(sa) && Convert.ToBoolean(sb);
                    i.Push(ans);
                }
                else if (c.Equals("||"))
                {
                    string sa = Convert.ToString(i.Pop());
                    string sb = Convert.ToString(i.Pop());
                    ans = Convert.ToBoolean(sa) || Convert.ToBoolean(sb);
                    i.Push(ans);
                }
                else if (c.Equals("=="))
                {
                    string sa = Convert.ToString(i.Pop());
                    string sb = Convert.ToString(i.Pop());
                    ans = sa == sb;
                    i.Push(ans);
                }
                else if (c.Equals("!="))
                {
                    string sa = Convert.ToString(i.Pop());
                    string sb = Convert.ToString(i.Pop());
                    ans = sa != sb;
                    i.Push(ans);
                }
                else if (c.Equals("<"))
                {
                    string sa = Convert.ToString(i.Pop());
                    string sb = Convert.ToString(i.Pop());
                    ans = Convert.ToInt32(sb) < Convert.ToInt32(sa);
                    i.Push(ans);
                }
                else if (c.Equals(">"))
                {
                    string sa = Convert.ToString(i.Pop());
                    string sb = Convert.ToString(i.Pop());
                    ans = Convert.ToInt32(sb) > Convert.ToInt32(sa);
                    i.Push(ans);
                }
                else if (c.Equals("<="))
                {
                    string sa = Convert.ToString(i.Pop());
                    string sb = Convert.ToString(i.Pop());
                    ans = Convert.ToInt32(sb) <= Convert.ToInt32(sa);
                    i.Push(ans);
                }
                else if (c.Equals(">="))
                {
                    string sa = Convert.ToString(i.Pop());
                    string sb = Convert.ToString(i.Pop());
                    ans = Convert.ToInt32(sb) >= Convert.ToInt32(sa);
                    i.Push(ans);
                }
                else if (c.Equals("in"))
                {
                    string sa = Convert.ToString(i.Pop());
                    string sb = Convert.ToString(i.Pop());
                    string[] saList = sa.Split(',');
                    ans = false;
                    for (int k = 0; k < saList.Length; k++)
                    {
                        ans = k == 0 ? (sb == saList[k]) : (ans || (sb == saList[k]));
                    }
                    i.Push(ans);
                }
                else
                {
                    i.Push(expression[j]);//po.Substring(j, 1)
                }
            }
            return Convert.ToBoolean(i.Pop());
        }

        public string InfixToPostfix(string expression)
        {
            string[] values = expression.Split(';');
            // Declaring a Stack from Standard template library in C++. 
            Stack<string> S = new Stack<string>();
            string postfix = ""; // Initialize postfix as empty string.
            for (int i = 0; i < values.Length; i++)
            {

                string ex = "";
                ex += values[i];

                // Scanning each character from left. 
                // If character is a delimitter, move on. 
                if (ex == " " || ex == ",") continue;

                // If character is operator, pop two elements from stack, perform operation and push the result back. 
                else if (IsOperator(ex))
                {
                    while (S.Count != 0 && S.Peek() != "(" && HasHigherPrecedence(S.Peek(), ex))
                    {
                        postfix += ';' + S.Peek();
                        S.Pop();
                    }
                    S.Push(ex);
                }
                // Else if character is an operand
                else if (!IsOperator(ex) && !IsLogicalOperator(ex) && ex != "(" && ex != ")")//Isoperand
                {
                    postfix += ';' + ex;
                }

                else if (ex == "(")
                {
                    S.Push(ex);
                }
                else if (ex == ")")
                {
                    while (S.Count != 0  && S.Peek() != "(") //s.empty()
                    {
                        postfix += ';' + S.Peek();
                        S.Pop();
                    }
                    S.Pop();
                }
                else if (IsLogicalOperator(ex))
                {
                    string db = "";
                    //string f = "";
                    //db += expression[i];
                    //f += expression[i + 1];
                    db = ex;

                    while (S.Count != 0 && S.Peek() != "(" && HasHigherPrecedence(S.Peek(), db))
                    {
                        postfix += ';' + S.Peek();
                        S.Pop();
                    }
                    S.Push(db);
                    //i++;
                }
            }

            while (S.Count != 0)
            {
                postfix += ';' + S.Peek();//top
                S.Pop();
            }

            return postfix.StartsWith(';') ? postfix.Remove(0,1): postfix;
        }

        // Function to verify whether a character is english letter or numeric digit. 
        // We are assuming in this solution that operand will be a single character
        //public bool IsOperand(string C)
        //{
        //    // if (C >= "A" && C <= "Z") return true;
        //    //return false;
        //    if (Regex.IsMatch(input, @"^[a-zA-Z0-9_]+$"))
        //        return true;
        //    else
        //        return false;


        //}

        // Function to verify whether a character is operator symbol or not. 
        public bool IsOperator(string C)
        {
            if (C == "+" || C == "-" || C == "*" || C == "/" || C == "%")
                return true;

            return false;
        }

        // Function to get weight of an operator. An operator with higher weight will have higher precedence. 
        public int GetOperatorWeight(string op)
        {
            int weight = -1;
            if (op == "&&" || op == "||" || op == "in")
                weight = 1;

            if (op == "==" || op == "!=")
                weight = 2;

            if (op == "<" || op == ">" || op == "<=" || op == ">=")
                weight = 3;
            if (op == "+" || op == "-")
                weight = 4;

            if (op == "/" || op == "%" || op == "/" || op == "*" || op == "(" || op == ")")
                weight = 5;
            return weight;
        }

        // Function to perform an operation and return output. 
        public bool HasHigherPrecedence(string op1, string op2)
        {
            int op1Weight = GetOperatorWeight(op1);
            int op2Weight = GetOperatorWeight(op2);

            // If operators have equal precedence, return true if they are left associative. 
            // return false, if right associative. 
            // if operator is left-associative, left one should be given priority. 

            if (op1Weight >= op2Weight)
                return true; //op1Weight op1Weight > op2Weight
            else
                return false;//op2Weight
        }

        public bool IsLogicalOperator(string db)
        {
            //string db = "";
            //string f = "";
            //db += c1;
            //f += c2;
            //db = db + f;

            if (db == "&&" || db == "||" || db == ">=" || db == "<=" || db == "!=" || db == "==" || db == ">" || db == "<" || db == "in")
            {
                //cout<<db;
                return true;
            }

            return false;
        }

        public string SaveTransactionInitiation(List<OfferTransactionInitiationRequest> offerTransactionInitiationRequests)
        {
            string result = string.Empty;
            try
            {
                var jwtaccesstoken = _tokenService.generateJwtTokenOnBankId();
                var client = new RestClient(_settings.Value.CardLinkAPI + "Reconcilation/SaveTransactionInitiation");
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("Authorization", "Bearer " + jwtaccesstoken);
                request.AddHeader("Content-Type", "application/json");
                string json = JsonConvert.SerializeObject(offerTransactionInitiationRequests);
                request.AddParameter("application/json", json, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                result = response.Content;
            }
            catch (Exception ex)
            {
                ErrorLogRequest errorLogRequest = new ErrorLogRequest
                {
                    ErrorMsg = ex.Message,
                    FullMsg = ex.StackTrace,
                    MethodName = "SaveTransactionInitiation",
                    ErrorLineNo = new StackTrace(ex, true).GetFrame(0).GetFileLineNumber().ToString(),
                    ExceptionType = ex.GetType().ToString()
                };
                _commonService.InsertErrorLog(errorLogRequest);
            }
            return result;
        }
    }
}
