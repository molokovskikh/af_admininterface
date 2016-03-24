using System;
using AdminInterface.Models.Billing;
using Common.Web.Ui.Helpers;

namespace AdminInterface.Models
{
	public class UserSearchItem
	{
		public uint UserId { get; set; }

		public uint PayerId { get; set; }

		public uint ClientId { get; set; }

		public string Login { get; set; }

		public string UserName { get; set; }

		public string ClientName { get; set; }

		public string RegionName { get; set; }

		public ulong RegionCode { get; set; }

		public uint AFVersion { get; set; }

		public SearchClientType ClientType { get; set; }

		public DateTime? UpdateDate { get; set; }

		[Style]
		public bool UpdateIsUncommited { get; set; }

		[Style]
		public bool AllowDownloadUnconfirmedOrders { get; set; }

		public bool SubmitOrders { get; set; }

		public bool IgnoreCheckMinOrder { get; set; }

		public bool ServiceDisabled { get; set; }

		public bool UserEnabled { get; set; }

		public string JuridicalName { get; set; }

		public bool IsLoginExists { get; set; }

		[Style]
		public bool NotExistsUser => !IsLoginExists;

		[Style]
		public bool FirstTable => SubmitOrders && IgnoreCheckMinOrder;

		[Style]
		public bool IsLocked { get; set; }

		[Style]
		public bool DisabledInAd { get; set; }

		[Style]
		public bool InvisibleClient { get; set; }

		public bool IsDrugstore => ClientType == SearchClientType.Drugstore;

		[Style]
		public bool DisabledByParent => ServiceDisabled;

		[Style]
		public bool SelfDisabled => !UserEnabled;

		[Style]
		public bool IsOldUserUpdate
		{
			get
			{
				if (ClientType == SearchClientType.Supplier)
					return false;
				if (UpdateDate != null)
					return DateTime.Now.Subtract(UpdateDate.Value).TotalDays >= 2;
				else
					return true;
			}
		}

		public override string ToString()
		{
			return UserId.ToString();
		}
	}
}