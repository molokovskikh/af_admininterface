using System;
using System.Linq;
using Castle.ActiveRecord;
using System.Collections.Generic;
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
	public class AuthorizationLogEntity : ActiveRecordLinqBase<AuthorizationLogEntity>
	{
		public AuthorizationLogEntity(uint id)
		{
			Id = id;
		}

		public AuthorizationLogEntity() {}

		[PrimaryKey("UserId", Generator = PrimaryKeyType.Assigned)]
		public virtual uint Id { get; set; }

		[Property]
		public virtual  DateTime? CITime { get; set; }

		[Property]
		public virtual  DateTime? AFTime { get; set; }

		[Property]
		public virtual  DateTime? AOLTime { get; set; }

		[Property]
		public virtual DateTime? IOLTime { get; set; }

		public static List<AuthorizationLogEntity> GetEntitiesByUsers(IEnumerable<User> users)
		{
			return FindAll(Expression.In("Id", users.Select(r => r.Id).ToArray())).ToList();
		}

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
