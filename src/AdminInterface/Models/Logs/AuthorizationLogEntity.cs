using System;
using System.Linq;
using Castle.ActiveRecord;
using System.Collections.Generic;
using Castle.ActiveRecord.Framework;
using Castle.ActiveRecord.Linq;
using NHibernate.Criterion;

namespace AdminInterface.Models.Logs
{
	public class LastServicesUsage
	{
		public DateTime Date;
		public string ShortServiceName;

		public static LastServicesUsage GetLastUsage(LastServicesUsage[] usages)
		{
			LastServicesUsage last = null;
			for (var i = 0; i < usages.Length; i++)
			{
				if (usages[i] == null)
					continue;
				if (last == null)
					last = usages[i];
				if (last.Date.CompareTo(usages[i].Date) < 0)
					last = usages[i];
			}
			return last;
		}
	}

	[ActiveRecord(Table = "AuthorizationDates", Schema = "logs", Lazy = true)]
	public class AuthorizationLogEntity
	{
		public AuthorizationLogEntity(User user)
		{
			User = user;
		}

		public AuthorizationLogEntity() {}

		[PrimaryKey("UserId", Generator = PrimaryKeyType.Foreign)]
		public virtual uint Id { get; set; }

		[OneToOne]
		public virtual User User { get; set; }

		[Property]
		public virtual  DateTime? CITime { get; set; }

		[Property]
		public virtual  DateTime? AFTime { get; set; }

		[Property]
		public virtual  DateTime? AOLTime { get; set; }

		[Property]
		public virtual DateTime? IOLTime { get; set; }

		[Property]
		public virtual DateTime? LastLogon { get; set; }

		public virtual string GetLastServicesUsage()
		{
			var usages = new [] {
				AFTime.HasValue ? new LastServicesUsage { Date = AFTime.Value, ShortServiceName = "AF" } : null,
				AOLTime.HasValue ? new LastServicesUsage { Date = AOLTime.Value, ShortServiceName = "AOL" } : null,
				CITime.HasValue ? new LastServicesUsage { Date = CITime.Value, ShortServiceName = "CI" } : null,
				IOLTime.HasValue ? new LastServicesUsage { Date = IOLTime.Value, ShortServiceName = "IOL" } : null,
			};
			var usage = LastServicesUsage.GetLastUsage(usages);
			if (usage == null)
				return "";
			return String.Format("{0} ({1})", usage.Date, usage.ShortServiceName);
		}
	}
}
