using System.Collections.Generic;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Suppliers;
using NHibernate;
using NHibernate.Linq;

namespace AdminInterface.Queries
{
	public class MessageQuery
	{
		public MessageQuery(params LogMessageType[] types)
		{
			Types = new List<LogMessageType>(types);
		}

		public MessageQuery()
		{
			Types = new List<LogMessageType> {
				LogMessageType.User,
				LogMessageType.System
			};
		}

		public IList<LogMessageType> Types { get; set; }

		public IList<AuditRecord> Execute(User user, ISession session)
		{
			var objectType = AuditRecord.GetLogObjectType(user);
			var serviceType = AuditRecord.GetLogObjectType(user.RootService);
			return session.Query<AuditRecord>()
				.Where(l => (l.ObjectId == user.Id && l.Type == objectType) || (l.ObjectId == user.RootService.Id && l.Type == serviceType))
				.Where(l => Types.Contains(l.MessageType))
				.OrderByDescending(l => l.WriteTime)
				.Fetch(l => l.Administrator)
				.ToList();
		}

		public IList<AuditRecord> Execute(Service service, ISession session)
		{
			return session.Query<AuditRecord>()
				.Where(l => l.Service == service)
				.Where(l => Types.Contains(l.MessageType))
				.OrderByDescending(l => l.WriteTime)
				.Fetch(l => l.Administrator)
				.ToList();
		}
	}
}