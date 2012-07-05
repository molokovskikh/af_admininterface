using System.Collections.Generic;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using AdminInterface.MonoRailExtentions;
using AdminInterface.Queries;
using AdminInterface.Security;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Helpers;
using NHibernate.SqlCommand;

namespace AdminInterface.Controllers
{
	[
		Secure,
		Helper(typeof (ViewHelper)),
		Helper(typeof (BindingHelper)),
		Filter(ExecuteWhen.BeforeAction, typeof (SecurityActivationFilter))
	]
	public class MonitoringController : AdminInterfaceController
	{
		public void Updates()
		{
			var sortMap = new Dictionary<string, string> {
				{"MethodName", "MethodName"},
				{"StartTime", "StartTime"},
				{"ShortName", "c.Name"},
				{"ClientCode", "c.Id"},
				{"User", "u.Name"}
			};

			var sortable = new Sortable(sortMap);
			BindObjectInstance(sortable, "filter");

			var criteria = DbSession.CreateCriteria<PrgDataLog>()
				.CreateAlias("User", "u", JoinType.InnerJoin)
				.CreateAlias("u.Client", "c", JoinType.InnerJoin);
			sortable.ApplySort(criteria);
			var logs = criteria.List<PrgDataLog>();

			PropertyBag["logs"] = logs;
			PropertyBag["filter"] = sortable;
		}

		public void Orders()
		{
			PropertyBag["Orders"] = new OrderFilter{NotSent = true}.Find();
		}
	}
}
