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

	public class AuthorizationLogEntityList : List<AuthorizationLogEntity>
	{
		public AuthorizationLogEntityList(List<AuthorizationLogEntity> list)
		{
			this.AddRange(list);
		}

		public AuthorizationLogEntity GetEntityByUserId(uint id)
		{
			AuthorizationLogEntity result = null;
			foreach (var entity in this)
				if(entity.Id == id)
				{
					result = entity;
					break;
				}
			return result;
		}
	}

	[ActiveRecord(Table = "logs.AuthorizationDates")]
	public class AuthorizationLogEntity : ActiveRecordLinqBase<AuthorizationLogEntity>
	{
		public AuthorizationLogEntity(uint id)
		{
			Id = id;
		}

		public AuthorizationLogEntity() {}

		[PrimaryKey("UserId", Generator = PrimaryKeyType.Assigned)]
		public uint Id { get; set; }

		[Property]
		public DateTime? CITime { get; set; }

		[Property]
		public DateTime? AFTime { get; set; }

		[Property]
		public DateTime? AOLTime { get; set; }

		[Property]
		public DateTime? IOLTime { get; set; }

		public static List<AuthorizationLogEntity> GetEntitiesByUsers(List<User> users)
		{
			return FindAll(Expression.In("Id", users.Select(r => r.Id).ToArray())).ToList();
		}

		public static LastServicesUsage GetLastServicesUsage(uint userId)
		{
			var logEntity = TryFind(userId);
			if (logEntity == null)
				return null;
			var usages = new [] {
                logEntity.AFTime.HasValue ? new LastServicesUsage { Date = logEntity.AFTime.Value, ShortServiceName = "AF" } : null,
                logEntity.AOLTime.HasValue ? new LastServicesUsage { Date = logEntity.AOLTime.Value, ShortServiceName = "AOL" } : null,
                logEntity.CITime.HasValue ? new LastServicesUsage { Date = logEntity.CITime.Value, ShortServiceName = "CI" } : null,
				logEntity.IOLTime.HasValue ? new LastServicesUsage { Date = logEntity.IOLTime.Value, ShortServiceName = "IOL" } : null,
			};
			return LastServicesUsage.GetLastUsage(usages);
		}
	}
}
