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
            // string merchantOffers = GetMerchantOffers();
            var transationFile = JsonConvert.DeserializeObject<List<TransactionDetailResponse>>(GetTransactionFileFromCardLink());
            var rulesList = JsonConvert.DeserializeObject<List<RuleNameResponse>>(GetRuleNames());
            var merchantOffersResponseList = JsonConvert.DeserializeObject<List<AllCustomerInterest>>(GetCustomerInterestOffers());
            //var merchantoffers = merchantOffersResponseList.Select(t => t.OfferDetails).ToList();
            List<OfferTransactionInitiationRequest> transactionList = new List<OfferTransactionInitiationRequest>();
            //transactionDetailResponse.ToList().Add(new TransactionDetailResponse { TenantId = Guid.Parse("f00c203c-a5d5-4db7-9c24-383f69fa43fd"), TransactionDetails = new TransactionDetail { AMOUNT = "500", DCARD = "test", LOCAL_DATE = "31-Dec-21", LOCAL_TIME = "0", MASK_PAN = "", Merchant_ID = "38R80714", Merchant_Name = "", MERCHANT_TYPE = "7999", REFNUM = "", TERMID = "" } });
            foreach (var item in transationFile)
            {
                foreach (var mol in merchantOffersResponseList.Where(t => t.merchant_id.Contains(item.TransactionDetails.Merchant_ID) && t.bankuser_id == item.TransactionDetails.VirtualId))
                {
                    // var merchantOfferDetail = merchantOffersResponseList.Where(t => t.merchant_id.Contains(item.TransactionDetails.Merchant_ID)).FirstOrDefault();
                    //var merchantofferrules = "min_order_amt;>=;1000;&&;max_discount_amt;<=;1000;&&;end_date;<=;2022-02-28;&&;from_date;>=;2022-02-01;&&;to_date;<=;2022-02-28";
                    var merchantofferrules = mol.offer_details.offer_rule;
                    if (merchantofferrules != null)
                    {
                        //if (merchantofferrules.ToLower().Contains("set_budget") && rulesList.Where(t => t.ruleField.ToLower() == "set_budget" && t.bankTransactionField.Where(t => t.name.ToLower() == "set_budget").Count() > 0).Count() > 0)
                        //{
                        //    //ToDO:transaction value hardcoded
                        //    merchantofferrules = merchantofferrules.ToLower().Replace("set_budget", "true".ToLower());
                        //}
                        //if (merchantofferrules.ToLower().Contains("discount_type") && rulesList.Where(t => t.ruleField.ToLower() == "discount_type" && t.bankTransactionField.Where(t => t.name.ToLower() == "discount_type").Count() > 0).Count() > 0)
                        //{
                        //    //ToDO:transaction value hardcoded
                        //    merchantofferrules = merchantofferrules.ToLower().Replace("discount_type", "true".ToLower());
                        //}
                        if (merchantofferrules.ToLower().Contains("publisher") && rulesList.Where(t => t.ruleField.ToLower() == "publisher" && t.bankTransactionField.Where(t => t.name.ToLower() == "publisher").Count() > 0).Count() > 0)
                        {
                            //ToDO:transaction value hardcoded
                            merchantofferrules = merchantofferrules.ToLower().Replace("publisher", item.BankName);
                        }

                        // min order amt 
                        if (merchantofferrules.ToLower().Contains("min_order_amt") && rulesList.Where(t => t.ruleField.ToLower() == "billing amount" && t.bankTransactionField.Where(t => t.name.ToLower() == "min_order_amt").Count() > 0).Count() > 0)
                        {
                            //ToDO:transaction value hardcoded
                            merchantofferrules = merchantofferrules.ToLower().Replace("min_order_amt", item.TransactionDetails.AMOUNT);
                        }
                        var discountAmount = "";
                        if (merchantofferrules.ToLower().Contains("max_discount_amt") && rulesList.Where(t => t.ruleField.ToLower() == "max discount amt" && t.bankTransactionField.Where(t => t.name.ToLower() == "max_discount_amt").Count() > 0).Count() > 0)
                        {
                            if (mol.offer_details.discount_details.discount_type == "2")
                            {
                                //ToDO: discount value hardcoded
                                discountAmount = (Convert.ToDecimal(item.TransactionDetails.AMOUNT) * (Convert.ToDecimal(mol.offer_details.discount_details.value) / 100)).ToString();
                            }
                            else
                            {
                                //ToDO: discount value hardcoded
                                discountAmount = (Convert.ToDecimal(item.TransactionDetails.AMOUNT) > 15 ? Convert.ToDecimal(item.TransactionDetails.AMOUNT) - Convert.ToDecimal(mol.offer_details.discount_details.value) : 0).ToString();
                            }

                            //ToDO:transaction value hardcoded
                            merchantofferrules = merchantofferrules.ToLower().Replace("max_discount_amt", discountAmount);
                        }

                        if (merchantofferrules.ToLower().Contains("offer_availability") && rulesList.Where(t => t.ruleField.ToLower() == "offeravailability" && t.bankTransactionField.Where(t => t.name.ToLower() == "offeravailability").Count() > 0).Count() > 0)
                        {
                            //ToDO:transaction value hardcoded
                            merchantofferrules = merchantofferrules.ToLower().Replace("offer_availability", "both,offline");
                        }

                        if (merchantofferrules.ToLower().Contains("end_date") && rulesList.Where(t => t.ruleField.ToLower() == "end_date" && t.bankTransactionField.Where(t => t.name.ToLower() == "end_date").Count() > 0).Count() > 0)
                        {
                            //ToDO:transaction value hardcoded
                            merchantofferrules = merchantofferrules.ToLower().Replace("end_date", Convert.ToDateTime(item.TransactionDetails.LOCAL_DATE).ToString("yyyy-MM-dd"));
                        }
                        if (merchantofferrules.ToLower().Contains("from_date") && rulesList.Where(t => t.ruleField.ToLower() == "from date" && t.bankTransactionField.Where(t => t.name.ToLower() == "from_date").Count() > 0).Count() > 0)
                        {
                            //ToDO:transaction value hardcoded
                            merchantofferrules = merchantofferrules.ToLower().Replace("from_date", Convert.ToDateTime(item.TransactionDetails.LOCAL_DATE).ToString("yyyy-MM-dd"));
                        }

                        if (merchantofferrules.ToLower().Contains("to_date") && rulesList.Where(t => t.ruleField.ToLower() == "to date" && t.bankTransactionField.Where(t => t.name.ToLower() == "to_date").Count() > 0).Count() > 0)
                        {
                            //ToDO:transaction value hardcoded
                            merchantofferrules = merchantofferrules.ToLower().Replace("to_date",Convert.ToDateTime(item.TransactionDetails.LOCAL_DATE).ToString("yyyy-MM-dd"));
                        }
                        if (merchantofferrules.ToLower().Contains("offer_status") && rulesList.Where(t => t.ruleField.ToLower() == "offerstatus" && t.bankTransactionField.Where(t => t.name.ToLower() == "offerstatus").Count() > 0).Count() > 0)
                        {
                            //ToDO:transaction value hardcoded
                            merchantofferrules = merchantofferrules.ToLower().Replace("offer_status", "published".ToLower());
                        }
                        //merchantofferrules = "500;>=;6777;&&;85;<=;4567;&&;offline;in;online,offline;&&;bom;in;all;&&;22-06-2022 17:51:31;<=;2022-01-31;&&;22-06-2022 17:52:09;>=;2022-01-31;&&;22-06-2022 17:52:14;<=;2022-01-31;&&;published;==;published";

                        string po = InfixToPostfix(merchantofferrules);
                        bool offerValid = ValidateRules(po);
                        if (offerValid)
                        {
                            OfferTransactionInitiationRequest otir = new OfferTransactionInitiationRequest
                            {
                                offer_id = (Guid)mol.id,
                                offer_transaction_details = new offerTransactiondetails
                                {
                                    discount_amount = discountAmount,
                                    cheg_comission_amount = String.Format("{0:0.00}", Convert.ToDecimal(discountAmount) * Convert.ToDecimal(mol.business_details.commission)/100),
                                    original_amount = item.TransactionDetails.AMOUNT,
                                    Cheg_commission = mol.business_details.commission,
                                    merchantId = item.TransactionDetails.Merchant_ID,
                                    cheg_id = mol.cheg_id,
                                    bankuser_id = mol.bankuser_id,
                                    bank_local_date = item.TransactionDetails.LOCAL_DATE,
                                    bank_local_time = item.TransactionDetails.LOCAL_TIME,
                                    merchant_name = item.TransactionDetails.Merchant_Name,
                                    merchant_type = item.TransactionDetails.MERCHANT_TYPE,
                                    ref_number = item.TransactionDetails.REFNUM,
                                    terminal_id = item.TransactionDetails.TERMID
                                },
                                tenant_id = item.TenantId,
                                status = "pending",
                                bankName = item.BankName
                            };
                            transactionList.Add(otir);
                        }
                    }
                }
            }
            SaveTransactionInitiation(transactionList);
        }
        public string GetCustomerInterestOffers()
        {
            string result = string.Empty;
            try
            {
                var jwtaccesstoken = _tokenService.generateJwtTokenOnBankId();
                var client = new RestClient(_settings.Value.CardLinkAPI + "Merchant/GetCustomerInterestOffers");
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
                    MethodName = "GetCustomerInterest",
                    ErrorLineNo = new StackTrace(ex, true).GetFrame(0).GetFileLineNumber().ToString(),
                    ExceptionType = ex.GetType().ToString()
                };
                _commonService.InsertErrorLog(errorLogRequest);
            }
            return result;
        }

        public string GetRuleNames()
        {
            string result = string.Empty;
            try
            {
                var jwtaccesstoken = _tokenService.generateJwtTokenOnBankId();
                var client = new RestClient(_settings.Value.CardLinkAPI + "Reconcilation/GetRuleNames");
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                request.AddHeader("Authorization", "Bearer " + jwtaccesstoken);
                request.AddHeader("Content-Type", "application/json");
                //request.AddParameter("bankName", bankName);
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
                var client = new RestClient(_settings.Value.CardLinkAPI + "Offers/MerchantOffers");
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("Authorization", "Bearer " + jwtaccesstoken);
                request.AddHeader("Content-Type", "application/json");
                //JObject jObjectbody = new JObject();
                //jObjectbody.Add("MerchantId", "0");
                //string json = JsonConvert.SerializeObject(jObjectbody);
                //request.AddParameter("application/json", json, ParameterType.RequestBody);
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
                    string sa = Convert.ToString(i.Pop());
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
                    // ans = Convert.ToInt32(sb) < Convert.ToInt32(sa);
                    try
                    {
                        ans = Convert.ToInt32(sb) < Convert.ToInt32(sa);
                    }
                    catch
                    {
                        ans = Convert.ToDateTime(sb) < Convert.ToDateTime(sa);
                    }
                    i.Push(ans);
                }
                else if (c.Equals(">"))
                {
                    string sa = Convert.ToString(i.Pop());
                    string sb = Convert.ToString(i.Pop());
                    //ans = Convert.ToInt32(sb) > Convert.ToInt32(sa);
                    try
                    {
                        ans = Convert.ToInt32(sb) > Convert.ToInt32(sa);
                    }
                    catch
                    {
                        ans = Convert.ToDateTime(sb) > Convert.ToDateTime(sa);
                    }
                    i.Push(ans);
                }
                else if (c.Equals("<="))
                {
                    string sa = Convert.ToString(i.Pop());
                    string sb = Convert.ToString(i.Pop());
                    //ans = Convert.ToInt32(sb) <= Convert.ToInt32(sa);
                    try
                    {
                        ans = Convert.ToInt32(sb) <= Convert.ToInt32(sa);
                    }
                    catch
                    {
                        ans = Convert.ToDateTime(sb) <= Convert.ToDateTime(sa);
                    }
                    i.Push(ans);
                }
                else if (c.Equals(">="))
                {
                    string sa = Convert.ToString(i.Pop());
                    string sb = Convert.ToString(i.Pop());
                    try
                    {
                        ans = Convert.ToInt32(sb) >= Convert.ToInt32(sa);
                    }
                    catch
                    {
                        ans = Convert.ToDateTime(sb) >= Convert.ToDateTime(sa);
                    }
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
                        ans = k == 0 ? (sb == saList[k] || saList[k].ToLower() =="all") : (ans || (sb == saList[k] || saList[k].ToLower() == "all"));
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
                    while (S.Count != 0 && S.Peek() != "(") //s.empty()
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

            return postfix.StartsWith(';') ? postfix.Remove(0, 1) : postfix;
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
            if (op == "&&" || op == "||" )
                weight = 1;

            if (op == "==" || op == "!=")
                weight = 2;

            if (op == "<" || op == ">" || op == "<=" || op == ">=" || op == "in")
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
