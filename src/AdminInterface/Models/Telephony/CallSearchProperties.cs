using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;

namespace AdminInterface.Models.Telephony
{
	public class CallSearchProperties
	{
		public string SearchText { get; set; }

		public CallType CallType { get; set; }

		public DateTime BeginDate { get; set; }

		public DateTime EndDate { get; set; }

		public CallSearchProperties()
		{
			SearchText = String.Empty;
			CallType = CallType.All;
		}

		public void Init()
		{
			SearchText = String.Empty;
			CallType = CallType.All;
			BeginDate = DateTime.Today.AddDays(-1);
			EndDate = DateTime.Today;
		}
	}
}
