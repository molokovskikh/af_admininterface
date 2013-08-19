using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdminInterface.Models;
using AdminInterface.MonoRailExtentions;
using AdminInterface.Security;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Models;
using NHibernate.Linq;
using NHibernate.Mapping;

namespace AdminInterface.Controllers
{
	[Filter(ExecuteWhen.BeforeAction, typeof(SecurityActivationFilter))]
	public class RegionsController : AdminInterfaceController
	{
		public void Index()
		{
			PropertyBag["regions"] = Region.All();
		}

		public void Edit(ulong id,
			[DataBind("DefaultRegions")] ulong[] defaultRegions,
			[DataBind("DefaultShowRegion")] ulong[] defaultShowRegion,
			[DataBind("SuppliersMarkup")] Markup[] suppliersMarkup,
			[DataBind("DrugstoreMarkup")] Markup[] drugstoreMarkup)
		{
			var region = DbSession.Load<Region>(id);

			var qsuppliersMarkup = DbSession.Query<Markup>()
				.Where(m => m.RegionId == region.Id && m.Type == 0).ToList();
			var qdrugstoreMarkup = DbSession.Query<Markup>()
				.Where(m => m.RegionId == region.Id && m.Type == 1).ToList();

			foreach (var limit in MarkupLimits.markupLimits) {
				if (!qsuppliersMarkup.Any(m => m.Begin == limit.Begin && m.End == limit.End)) {
					var markup = new Markup {
						RegionId = region.Id,
						Type = 0,
						Begin = limit.Begin,
						End = limit.End
					};
					qsuppliersMarkup.Add(markup);
					DbSession.Save(markup);
				}
				if (!qdrugstoreMarkup.Any(m => m.Begin == limit.Begin && m.End == limit.End)) {
					var markup = new Markup {
						RegionId = region.Id,
						Type = 1,
						Begin = limit.Begin,
						End = limit.End
					};
					qdrugstoreMarkup.Add(markup);
					DbSession.Save(markup);
				}
			}

			if (IsPost) {
				BindObjectInstance(region, "region");
				region.DefaultRegionMask = defaultRegions.Aggregate(0UL, (v, a) => a + v);
				region.DefaultShowRegionMask = defaultShowRegion.Aggregate(0UL, (v, a) => a + v);
				if (IsValid(region)) {
					DbSession.Save(region);
					foreach (var markup in qsuppliersMarkup) {
						markup.Value = suppliersMarkup.Where(x => x.Id == markup.Id).FirstOrDefault().Value;
						DbSession.Update(markup);
					}
					foreach (var markup in qdrugstoreMarkup) {
						markup.Value = drugstoreMarkup.Where(x => x.Id == markup.Id).FirstOrDefault().Value;
						DbSession.Update(markup);
					}
					Notify("Сохранено");
					RedirectToReferrer();
				}
			}
			PropertyBag["region"] = region;
			PropertyBag["AllRegions"] = Region.All();
			PropertyBag["SuppliersMarkup"] = qsuppliersMarkup;
			PropertyBag["DrugstoreMarkup"] = qdrugstoreMarkup;
		}

		public void ShowRegions(uint? clientId, ulong? homeRegionId)
		{
			ShowRegions(clientId, homeRegionId, true, false);
		}

		public void ShowRegions(uint? clientId, ulong? homeRegionId, bool showDefaultRegions, bool showNonDefaultRegions)
		{
			var allRegions = Region.FindAll();
			if (homeRegionId.HasValue) {
				var homeRegion = DbSession.Load<Region>(homeRegionId.Value);
				if (showDefaultRegions)
					PropertyBag["defaultRegions"] = allRegions.Where(region => (region.Id & homeRegion.DefaultShowRegionMask) > 0);
				if (showNonDefaultRegions)
					PropertyBag["nonDefaultRegions"] = allRegions.Where(region => (region.Id & homeRegion.DefaultShowRegionMask) <= 0);
				PropertyBag["homeRegion"] = DbSession.Load<Region>(homeRegionId);
			}
			else if (clientId.HasValue) {
				var client = DbSession.Load<Client>(clientId.Value);
				var drugstore = client.Settings;

				if (showDefaultRegions)
					PropertyBag["defaultRegions"] = allRegions.Where(region =>
						(region.Id & client.MaskRegion) > 0 ||
							(region.Id & drugstore.OrderRegionMask) > 0);
				if (showNonDefaultRegions)
					PropertyBag["nonDefaultRegions"] = allRegions.Where(region =>
						(region.Id & client.MaskRegion) <= 0 ||
							(region.Id & drugstore.OrderRegionMask) <= 0);
				PropertyBag["homeRegion"] = client.HomeRegion;
				PropertyBag["drugstore"] = drugstore;
			}
			CancelLayout();
		}

		public void DefaultRegions(ulong homeRegionId, bool singleRegions)
		{
			var homeRegion = DbSession.Load<Region>(homeRegionId);
			var regions = Region.FindAll()
				.Where(region => (region.Id & homeRegion.DefaultShowRegionMask) > 0)
				.OrderBy(region => region.Name)
				.ToArray();
			PropertyBag["singleRegions"] = singleRegions;
			PropertyBag["regions"] = regions;
			PropertyBag["homeRegionId"] = homeRegion.Id;
			CancelLayout();
		}

		public void DefaultRegions(ulong homeRegionId, uint clientId)
		{
			var homeRegion = DbSession.Load<Region>(homeRegionId);
			var client = DbSession.Load<Client>(clientId);
			var drugstore = client.Settings;
			var regions = Region.FindAll()
				.Where(region => (region.Id & homeRegion.DefaultShowRegionMask) > 0 ||
					((region.Id & client.MaskRegion) > 0) ||
					((region.Id & drugstore.OrderRegionMask) > 0))
				.OrderBy(region => region.Name)
				.ToArray();
			PropertyBag["regions"] = regions;
			PropertyBag["client"] = client;
			PropertyBag["drugstore"] = drugstore;
			PropertyBag["homeRegionId"] = homeRegion.Id;
			CancelLayout();
		}
	}
}