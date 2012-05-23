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

		public IList<ClientInfoLogEntity> Execute(User user, ISession session)
		{
			var objectType = ClientInfoLogEntity.GetLogObjectType(user);
			var serviceType = ClientInfoLogEntity.GetLogObjectType(user.RootService);
			return session.Query<ClientInfoLogEntity>()
				.Where(l => (l.ObjectId == user.Id && l.Type == objectType) || (l.ObjectId == user.RootService.Id && l.Type == serviceType))
				.Where(l => Types.Contains(l.MessageType))
				.OrderByDescending(l => l.WriteTime)
				.Fetch(l => l.Administrator)
				.ToList();
		}

		public IList<ClientInfoLogEntity> Execute(Service service, ISession session)
		{
			return session.Query<ClientInfoLogEntity>()
				.Where(l => l.Service == service)
				.Where(l => Types.Contains(l.MessageType))
				.OrderByDescending(l => l.WriteTime)
				.Fetch(l => l.Administrator)
				.ToList();
		}
	}
}