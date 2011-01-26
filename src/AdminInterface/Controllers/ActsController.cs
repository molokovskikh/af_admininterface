using System;
using System.Diagnostics;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using AdminInterface.Models.Billing;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Common.Tools;
using Common.Web.Ui.Helpers;

namespace AdminInterface.Controllers
{
	[
		Layout("GeneralWithJQueryOnly"),
		Helper(typeof(BindingHelper))
	]
	public class ActsController : ARSmartDispatcherController
	{	
		public void Index([ARDataBind("filter", AutoLoad = AutoLoadBehavior.NullIfInvalidKey)] PayerDocumentFilter filter)
		{
			PropertyBag["filter"] = filter;
			PropertyBag["acts"] = filter.Find<Act>();
			PropertyBag["buildFilter"] = new DocumentBuilderFilter();

			PropertyBag["printers"] = PrinterSettings.InstalledPrinters.Cast<string>().Where(p => p.Contains("Бух"));
		}

		public void Build([ARDataBind("buildFilter", AutoLoad = AutoLoadBehavior.NullIfInvalidKey)] DocumentBuilderFilter filter, DateTime actDate)
		{
			var invoices = filter.Find<Invoice>();
			var acts = invoices.GroupBy(i => i.Payer).Select(g => new Act(actDate, g.ToArray()));
			foreach (var act in acts)
				act.Save();
			RedirectToAction("Index", filter.ToUrl());
		}

		public void Process([ARDataBind("acts", AutoLoadBehavior.Always)] Act[] acts)
		{
			if (Form["delete"] != null)
			{
				foreach (var act in acts)
					act.Delete();

				Flash["message"] = "Удалено";
				RedirectToReferrer();
			}
			if (Form["print"] != null)
			{
				var printer = Form["printer"];
				var path = @"U:\Apps\Printer\";
#if DEBUG
				path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\Printer\bin\debug\");
#endif
#if !DEBUG
				var info = new ProcessStartInfo(Path.Combine(path, "Printer.exe"),
					String.Format("act \"{0}\" \"{1}\"", printer, acts.Implode(a => a.Id.ToString())));
				var process = System.Diagnostics.Process.Start(info);
				process.WaitForExit(30*1000);
#endif
				Flash["message"] = "Отправлено на печать";
				RedirectToReferrer();
			}
		}

		public void Print(uint id)
		{
			CancelLayout();
			PropertyBag["act"] = Act.Find(id);
		}
	}
}