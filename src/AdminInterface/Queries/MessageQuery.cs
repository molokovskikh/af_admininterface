using System.Collections.Generic;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Suppliers;
using Common.Web.Ui.Models.Audit;
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
			var userAudit = session.Query<AuditRecord>()
				.Where(l => (l.ObjectId == user.Id && l.Type == objectType) || (l.ObjectId == user.RootService.Id && l.Type == serviceType))
				.Where(l => Types.Contains(l.MessageType))
				.OrderByDescending(l => l.WriteTime)
				.Fetch(l => l.Administrator)
				.ToList();
			return userAudit.Concat(
				ForPayer(user.Payer, session)
					.Where(u => !(u.ShowOnlyPayer && u.Type == LogObjectType.User && u.ObjectId == user.Id)))
				.OrderByDescending(o => o.WriteTime)
				.ToList();
		}

		public IList<AuditRecord> ExecuteUser(User user, ISession session)
		{
			var userAudit = session.Query<AuditRecord>()
				.Where(l => (l.ObjectId == user.Id && l.Type == LogObjectType.User))
				.Where(l => Types.Contains(l.MessageType))
				.OrderByDescending(l => l.WriteTime)
				.Fetch(l => l.Administrator)
				.ToList();
			return userAudit.Concat(
				ForPayer(user.Payer, session)
					.Where(u => !(u.ShowOnlyPayer && u.Type == LogObjectType.User && u.ObjectId == user.Id)))
				.OrderByDescending(o => o.WriteTime).ToList();
		}

		public IList<AuditRecord> Execute(Service service, ISession session)
		{
			var serviceAudit = session.Query<AuditRecord>()
				.Where(l => l.Service == service)
				.Where(l => Types.Contains(l.MessageType))
				.OrderByDescending(l => l.WriteTime)
				.Fetch(l => l.Administrator)
				.ToList();
			if (service.IsClient()) {
				return serviceAudit.Concat(
					((Client)service).Payers.SelectMany(p => ForPayer(p, session)
						.Where(u => !(u.ShowOnlyPayer && u.Type == LogObjectType.Client && u.ObjectId == service.Id))))
					.OrderByDescending(o => o.WriteTime)
					.ToList();
			}
			else {
				return serviceAudit.Concat(
					ForPayer(((Supplier)service).Payer, session)
						.Where(u => !(u.ShowOnlyPayer && u.Type == LogObjectType.Supplier && u.ObjectId == service.Id)))
					.OrderByDescending(o => o.WriteTime)
					.ToList();
			}
		}

		public IList<AuditRecord> ForPayer(Payer payer, ISession session)
		{
			if (payer != null && Types.Contains(LogMessageType.Payer)) {
				var payerMessages = AuditLogRecord.GetLogs(session, payer, false);
				return payerMessages.Select(m => new AuditRecord {
					Message = m.Message,
					ObjectId = m.ObjectId,
					Name = m.Name,
					Type = m.LogType,
					WriteTime = m.LogTime,
					UserName = m.OperatorName,
					MessageType = LogMessageType.System,
					ShowOnlyPayer = m.ShowOnlyPayer
				}).ToList();
			}
			return new List<AuditRecord>();
		}
	}
}