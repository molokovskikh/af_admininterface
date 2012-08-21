using System;
using AdminInterface.Models.Logs;
using AdminInterface.Security;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Helpers;

namespace AdminInterface.Controllers
{
	[
		Secure,
		Helper(typeof(ViewHelper)),
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