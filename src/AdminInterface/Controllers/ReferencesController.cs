﻿using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using AdminInterface.Models.Billing;
using AdminInterface.MonoRailExtentions;
using Castle.ActiveRecord;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Common.Web.Ui.ActiveRecordExtentions;
using Common.Web.Ui.Helpers;
using NHibernate.Criterion;

namespace AdminInterface.Controllers
{
	public class Tab
	{
		public string Name { get; set; }
		public string Id { get; set; }
		public Type Type { get; set; }
		public IList Items { get; set; }

		public Tab(Type type)
		{
			Id = type.Name;
			Name = BindingHelper.GetDescription(type);
			Type = type;
			Items = ArHelper.WithSession(
				s => s.CreateCriteria(type)
					.AddOrder(Order.Asc("Name"))
					.List());
		}
	}

	public class ReferencesController : AdminInterfaceController
	{
		public ReferencesController()
		{
			SetBinder(new ARDataBinder());
		}

		public void Index()
		{
			var settings = new[] {
				new Tab(typeof(IgnoredInn)),
				new Tab(typeof(Nomenclature)),
			};

			if (IsPost) {
				var setting = settings.FirstOrDefault(s => Form[s.Id] != null);
				if (setting == null) {
					RedirectToReferrer();
					return;
				}

				((ARDataBinder)Binder).AutoLoad = AutoLoadBehavior.NewInstanceIfInvalidKey;
				var forSave = (IList)BindObject(ParamStore.Form, setting.Type.MakeArrayType(), "items");

				if (IsValid(forSave)) {
					var items = setting.Items;
					var forDelete = items.Cast<dynamic>().Where(r => !forSave.Cast<dynamic>().Any(n => n.Id == r.Id));
					foreach (var deleted in forDelete)
						DbSession.Delete(deleted);
					foreach (var item in forSave)
						DbSession.Save(item);

					Notify("Сохранено");
					//ie не передает в referer hash по этому формируем вручную
					//RedirectToReferrer();
					RedirectToUrl(String.Format("~/References/#tab-{0}", setting.Id));
				}
				else {
					setting.Items = forSave.Cast<object>().ToList();
				}
			}

			PropertyBag["settings"] = settings;
		}
	}
}