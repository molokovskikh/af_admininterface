using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AdminInterface.Helpers;
using AdminInterface.Models.Security;
using Castle.ActiveRecord;
using Common.Web.Ui.Helpers;
using Common.MySql;

namespace AdminInterface.Models
{
	[ActiveRecord(SchemaAction = "none")]
	public class UserSearchItem : ActiveRecordBase<UserSearchItem>
	{
		[PrimaryKey]
		public uint UserId { get; set; }

		[Property]
		public uint PayerId { get; set; }

		[Property]
		public uint ClientId { get; set; }

		[Property]
		public string Login { get; set; }

		[Property]
		public string UserName { get; set; }

		[Property]
		public string ClientName { get; set; }

		[Property]
		public string RegionName { get; set; }

		[Property]
		public uint AFVersion { get; set; }

		[Property]
		public SearchSegment Segment { get; set; }

		[Property]
		public SearchClientType ClientType { get; set; }

		[Property]
		public DateTime? UpdateDate { get; set; }

		[Property]
		public bool UpdateIsUncommited { get; set; }

		[Property]
		public bool Disabled { get; set; }

		[Property]
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
	}
}
