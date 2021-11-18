using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CardLinkScheduler.DTOs
{
	/// <summary>
	/// Input request parameters for Error Logs. 
	/// </summary>

	public class Rules
    {
		public IList<AllowedMerchant> Merchant { get; set; }
		public IList<AllowedBank> allowedBanks { get; set; }
    }

	public class AllowedMerchant
    {
		public string Name { get; set; }
		public string Operand { get; set; }
    }

	public class AllowedBank
	{
		public string Name { get; set; }
		public string Operand { get; set; }
	}

	public class MinBillingAmount
	{
		public string Amount { get; set; }
		public string Operand { get; set; }
	}


	public class MaxDiscountAmount
	{
		public string Amount { get; set; }
		public string Operand { get; set; }
	}

	public class MonthlyAmountLimit
	{
		public string Amount { get; set; }
		public string Operand { get; set; }
	}

	public class MonthlyTransactionLimit
	{
		public string Amount { get; set; }
		public string Operand { get; set; }
	}

	public class RecurrentUSer
	{
		public string Amount { get; set; }
		public string Operand { get; set; }
	}
}
