using System;
using System.Drawing.Printing;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Helpers;

namespace AdminInterface.Controllers
{
	[
		Layout("GeneralWithJQueryOnly"),
		Helper(typeof(BindingHelper))
	]
	public class ActsController : ARSmartDispatcherController
	{	
		public void Index([DataBind("filter")] PayerDocumentFilter filter)
		{
			PropertyBag["filter"] = filter;
			PropertyBag["acts"] = filter.Find<Act>();
			PropertyBag["buildFilter"] = new DocumentBuilderFilter();

			PropertyBag["printers"] = PrinterSettings.InstalledPrinters.Cast<string>().Where(p => p.Contains("Бух"));
		}

		public void Build([DataBind("filter")] DocumentBuilderFilter filter, DateTime actDate)
		{
			var invoices = filter.Find<Invoice>();
			var acts = invoices.GroupBy(i => i.Payer).Select(g => new Act(actDate, g.ToArray()));
			foreach (var act in acts)
				act.Save();
			RedirectToAction("Index", filter.ToUrl());
		}

		public void Process([DataBind("acts")] Act[] acts)
		{
			if (Form["submit"] == "Удалить")
			{
				foreach (var act in acts)
					act.Delete();

				RedirectToReferrer();
			}
			if (Form["submit"] == "Напечатать")
			{

				RedirectToReferrer();
			}
		}

		public void Print(uint id)
		{
			PropertyBag["act"] = Act.Find(id);
		}
	}
}