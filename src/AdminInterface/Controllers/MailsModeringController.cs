using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdminInterface.Models.Suppliers;
using AdminInterface.MonoRailExtentions;
using AdminInterface.Queries;
using Castle.MonoRail.Framework;
using NHibernate.Linq;

namespace AdminInterface.Controllers
{
	public class MailsModeringController : AdminInterfaceController
	{
		public void Index([DataBind("filter")] MiniMailFilter filter)
		{
			PropertyBag["filter"] = filter;
		}

		[return: JSONReturnBinder]
		public object GetSullierList(string term)
		{
			uint id = 0;
			uint.TryParse(term, out id);
			return DbSession.Query<Supplier>().Where(c =>
				(c.Name.Contains(term) || c.Id == id))
				.ToList()
				.Select(c => new { id = c.Id, label = c.Name })
				.ToList();
		}
	}
}