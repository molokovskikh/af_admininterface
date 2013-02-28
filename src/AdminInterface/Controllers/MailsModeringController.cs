using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdminInterface.Models.Documents;
using AdminInterface.Models.Suppliers;
using AdminInterface.MonoRailExtentions;
using AdminInterface.Queries;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Helpers;
using NHibernate.Linq;

namespace AdminInterface.Controllers
{
	[Helper(typeof(TableHelper), "tableHelper")]
	[Helper(typeof(PaginatorHelper), "paginator")]
	public class MailsModeringController : AdminInterfaceController
	{
		public void Index()
		{
			var filter = BindFilter<MiniMailFilter, BaseItemForTable>();
			FindFilter(filter);
		}

		public void ShowMail(uint mailId)
		{
			var mail = DbSession.Get<Mail>(mailId);
			PropertyBag["mail"] = mail;
			PropertyBag["recipients"] = mail.Recipients.GroupBy(g => g.Type).Select(r => new { r.Key, items = r.ToList() });
			CancelLayout();
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