using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CardLinkScheduler.DTOs
{
	/// <summary>
	/// Input request parameters for Error Logs. 
	/// </summary>
	public sealed class ErrorLogRequest
	{
		public string SiteName { get; set; }
		public string Source { get; set; }
		public string MethodName { get; set; }
		public string ErrorMsg { get; set; }
		public string FullMsg { get; set; }
		public string ErrorLineNo { get; set; }
		public string ExceptionType { get; set; }
		public string ApiKey { get; set; }
		public string Url { get; set; }

	}
}
