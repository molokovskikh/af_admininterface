using System;
using AdminInterface.Controllers.Filters;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Common.Tools;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;

namespace AdminInterface.Controllers
{
	[Helper(typeof(BindingHelper))]
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

			PropertyBag["printers"] = Printer.All();
		}

		public void PrintIndex([DataBind("filter")] PayerDocumentFilter filter)
		{
			LayoutName = "Print";
			if (filter.Region != null && filter.Region.Id == 0)
				filter.Region = null;
			else if (filter.Region != null)
				filter.Region = Region.Find(filter.Region.Id);

			if (filter.Recipient != null && filter.Recipient.Id == 0)
				filter.Recipient = null;
			else if (filter.Recipient != null)
				filter.Recipient = Recipient.Find(filter.Recipient.Id);

			PropertyBag["filter"] = filter;
			PropertyBag["acts"] = filter.Find<Act>();
		}

		public void Build([ARDataBind("buildFilter", AutoLoad = AutoLoadBehavior.NullIfInvalidKey)] DocumentBuilderFilter filter, DateTime actDate)
		{
			var invoices = filter.Find<Invoice>();
			foreach (var act in Act.Build(invoices, actDate))
				act.Save();

			RedirectToAction("Index", filter.ToDocumentFilter().GetQueryString());
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
				var arguments = String.Format("act \"{0}\" \"{1}\"", printer, acts.Implode(a => a.Id));
				Printer.Execute(arguments);
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
					act.CalculateSum();
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