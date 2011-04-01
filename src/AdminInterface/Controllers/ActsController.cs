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
		public void Index([DataBind("filter")] PayerDocumentFilter filter)
		{
			if (filter.Region != null && filter.Region.Id == 0)
				filter.Region = null;
			if (filter.Recipient != null && filter.Recipient.Id == 0)
				filter.Recipient = null;

			PropertyBag["filter"] = filter;
			PropertyBag["acts"] = filter.Find<Act>();
			PropertyBag["buildFilter"] = new DocumentBuilderFilter();

			PropertyBag["printers"] = PrinterSettings.InstalledPrinters
				.Cast<string>()
#if !DEBUG
				.Where(p => p.Contains("Бух"))
				.OrderBy(p => p)
#endif
				.ToList();
		}

		public void Build([ARDataBind("buildFilter", AutoLoad = AutoLoadBehavior.NullIfInvalidKey)] DocumentBuilderFilter filter, DateTime actDate)
		{
			var invoices = filter.Find<Invoice>();
			foreach (var act in Act.Build(invoices, actDate))
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
				var info = new ProcessStartInfo(Path.Combine(path, "Printer.exe"),
					String.Format("act \"{0}\" \"{1}\"", printer, acts.Implode(a => a.Id.ToString())));
				var process = System.Diagnostics.Process.Start(info);
				process.WaitForExit(30*1000);
				Flash["message"] = "Отправлено на печать";
				RedirectToReferrer();
			}
		}

		public void Edit(uint id)
		{
			Binder.Validator = Validator;

			var act = Act.Find(id);
			PropertyBag["act"] = act;

			if (IsPost)
			{
				BindObjectInstance(act, "act");
				if (!HasValidationError(act))
				{
					act.Save();
					Flash["Message"] = "Сохранено";
					RedirectUsingRoute("Acts", "Edit", new {act.Id});
				}
			}
		}

		public void Print(uint id)
		{
			LayoutName = "Print";
			PropertyBag["act"] = Act.Find(id);
			PropertyBag["doc"] = PropertyBag["act"];
		}
	}
}