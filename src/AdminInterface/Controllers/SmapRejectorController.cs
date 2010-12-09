using System;
using AdminInterface.Helpers;
using AdminInterface.Models.Logs;
using AdminInterface.Security;
using Castle.MonoRail.Framework;

namespace AdminInterface.Controllers
{
	[
		Secure,
		Helper(typeof(ViewHelper)),
		Layout("GeneralWithJQueryOnly"),
	]
	public class SmapRejectorController : SmartDispatcherController
	{
		public void Show()
		{
			Search("", DateTime.Today, DateTime.Today);
			RenderView("Search");
		}

		public void Search(string searchText, DateTime fromDate, DateTime toDate)
		{
			PropertyBag["fromDate"] = fromDate;
			PropertyBag["toDate"] = toDate;
			PropertyBag["searchText"] = searchText;
			PropertyBag["rejects"] = RejectedEmail.Find(searchText, fromDate, toDate);
		}
	}
}
