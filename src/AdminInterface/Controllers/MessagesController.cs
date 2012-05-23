using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AdminInterface.Helpers;
using AdminInterface.Models.Logs;
using AdminInterface.Security;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.NHibernateExtentions;
using NHibernate.Criterion;
using NHibernate.SqlCommand;

namespace AdminInterface.Controllers
{
	public class MessageFilter : Sortable
	{
		public DatePeriod Period { get; set; }
		public string SearchText { get; set; }
		[Description("Тип сообщения:")]
		public IList<LogMessageType> Types { get; set; }

		public MessageFilter()
		{
			SortKeyMap = new Dictionary<string, string>{
				{"Messages.WriteTime", "WriteTime"},
				{"Messages.Operator", "UserName"},
				{"Messages.Service.Id", "s.Id"},
				{"Messages.Service.Name", "s.Name"},
				{"Messages.Type", "Type"},
				{"Messages.ObjectId", "ObjectId"},
				{"Messages.Name", "Name"}
			};
			SortBy = "WriteTime";
			SortDirection = "desc";
			Period = new DatePeriod(DateTime.Today.AddDays(-7), DateTime.Today);
			Types = new List<LogMessageType>{LogMessageType.User, LogMessageType.System};
		}

		public IList<ClientInfoLogEntity> Find()
		{
			return ArHelper.WithSession(s => {
				uint id;
				uint.TryParse(SearchText, out id);
				var query = s.QueryOver<ClientInfoLogEntity>()
					.Where(l => l.WriteTime >= Period.Begin && l.WriteTime <= Period.End.AddDays(1))
					.Where(l => l.MessageType.IsIn(Types.ToArray()))
					.And(
						Restrictions.On<ClientInfoLogEntity>(l => l.Message).IsLike(SearchText, MatchMode.Anywhere) ||
						Restrictions.On<ClientInfoLogEntity>(l => l.Name).IsLike(SearchText, MatchMode.Anywhere) ||
						Restrictions.On<ClientInfoLogEntity>(l => l.ObjectId).IsLike(id)
					);

				query.RootCriteria
					.CreateCriteria("Service", "s", JoinType.InnerJoin)
					.Add(Expression.Sql("s1_.HomeRegion & " + SecurityContext.Administrator.RegionMask + " > 0"));

				ApplySort(query.RootCriteria);
				query.Fetch(c => c.Administrator);
				return query.List<ClientInfoLogEntity>();
			});
		}
	}

	public class MessagesController : SmartDispatcherController
	{
		public void Index([DataBind("filter")] MessageFilter filter)
		{
			PropertyBag["filter"] = filter;
			PropertyBag["messages"] = filter.Find();
		}
	}
}