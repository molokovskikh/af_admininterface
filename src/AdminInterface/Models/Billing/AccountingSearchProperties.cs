using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace AdminInterface.Models.Billing
{
	public enum AccountingSearchBy
	{
		[Description("Автоматически")] Auto,
		[Description("По адресу")] ByAddress,
		[Description("По пользователю")] ByUser,
		[Description("По клиенту")] ByClient,
		[Description("По плательщику")] ByPayer,
	}

	public class AccountingSearchProperties
	{
		public ulong RegionId { get; set; }

		public string SearchText { get; set; }

		public DateTime? BeginDate { get; set; }

		public DateTime? EndDate { get; set; }

		public AccountingSearchBy SearchBy { get; set; }

		public bool IsAutoSearch
		{
			get { return (SearchBy == AccountingSearchBy.Auto); }
		}

		public bool IsSearchByAddress
		{
			get { return (SearchBy == AccountingSearchBy.ByAddress); }
		}

		public bool IsSearchByUser
		{
			get { return (SearchBy == AccountingSearchBy.ByUser); }
		}

		public bool IsSearchByClient
		{
			get { return (SearchBy == AccountingSearchBy.ByClient); }
		}

		public bool IsSearchByPayer
		{
			get { return (SearchBy == AccountingSearchBy.ByPayer); }
		}
	}
}