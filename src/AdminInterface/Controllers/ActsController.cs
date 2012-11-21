﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AdminInterface.Controllers.Filters;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.MonoRailExtentions;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Common.Tools;
using Common.Web.Ui.ActiveRecordExtentions;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using Common.Web.Ui.MonoRailExtentions;
using NHibernate.Linq;

namespace AdminInterface.Controllers
{
	[
		Helper(typeof(BindingHelper)),
		Helper(typeof(PaginatorHelper), "paginator"),
	]
	public class ActsController : ARSmartDispatcherController
	{
		public void Index([SmartBinder] PayerDocumentFilter filter)
		{
			PropertyBag["filter"] = filter;
			PropertyBag["acts"] = filter.Find<Act>();
			PropertyBag["buildFilter"] = new DocumentBuilderFilter();

			PropertyBag["printers"] = Printer.All();
		}

		public void PrintIndex([SmartBinder] PayerDocumentFilter filter)
		{
			LayoutName = "Print";
			PropertyBag["filter"] = filter;
			PropertyBag["acts"] = filter.Find<Act>();
		}

		public void Build([ARDataBind("buildFilter", AutoLoad = AutoLoadBehavior.NullIfInvalidKey)] DocumentBuilderFilter filter, DateTime actDate)
		{
			var sourceInvoices = filter.Find<Invoice>();
			var invoices = ArHelper.WithSession(s => sourceInvoices
				.Where(i => !s.Query<Act>().Any(a => a.Payer == i.Payer && a.Period == i.Period)).ToList());
			var createdTime = DateTime.Now;
			foreach (var act in Act.Build(invoices, actDate)) {
				act.Save();
			}
			var destinationFilter = filter.ToDocumentFilter();
			destinationFilter.CreatedOn = createdTime;
			RedirectToAction("Index", destinationFilter.GetQueryString());
		}

		public void Process([ARDataBind("acts", AutoLoadBehavior.Always)] Act[] acts)
		{
			if (Form["delete"] != null) {
				foreach (var act in acts)
					act.Delete();

				Flash["message"] = "Удалено";
				RedirectToReferrer();
			}
			if (Form["print"] != null) {
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
			PropertyBag["references"] = Nomenclature.Queryable.OrderBy(n => n.Name).ToList();

			if (IsPost) {
				RecreateOnlyIfNullBinder.Prepare(this, "act.Parts");

				BindObjectInstance(act, "act");
				if (!HasValidationError(act)) {
					act.CalculateSum();
					act.Save();
					Flash["Message"] = "Сохранено";
					RedirectUsingRoute("Acts", "Edit", new { act.Id });
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