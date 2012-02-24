using System;
using AdminInterface.Models.Billing;

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

		public uint AFVersion { get; set; }

		public SearchClientType ClientType { get; set; }

		public DateTime? UpdateDate { get; set; }

		public bool UpdateIsUncommited { get; set; }

		public bool ServiceDisabled { get; set; }

		public bool UserEnabled { get; set; }

		public string JuridicalName { get; set; }

		public bool IsLoginExists { get; set; }

		public bool IsLocked { get; set; }

		public bool IsDisabled { get; set; }

		public bool IsDrugstore
		{
			get
			{
				return ClientType == SearchClientType.Drugstore;
			}
		}

		public bool Disabled
		{
			get
			{
				return ServiceDisabled || !UserEnabled;
			}
		}

		public override string ToString()
		{
			return UserId.ToString();
		}
	}
}
