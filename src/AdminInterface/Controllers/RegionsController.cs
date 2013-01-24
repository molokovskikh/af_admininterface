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
			var region = Region.Find(id);
			if (IsPost) {
				BindObjectInstance(region, "region");
				region.DefaultRegionMask = defaultRegions.Aggregate(0UL, (v, a) => a + v);
				region.DefaultShowRegionMask = defaultShowRegion.Aggregate(0UL, (v, a) => a + v);
				if (IsValid(region)) {
					region.Save();
					foreach (var markup in drugstoreMarkup) {
						DbSession.Update(markup);
					}
					foreach (var markup in suppliersMarkup) {
						DbSession.Update(markup);
					}
					Notify("Сохранено");
					RedirectToReferrer();
				}
			}
			PropertyBag["region"] = region;
			PropertyBag["AllRegions"] = Region.All();
			PropertyBag["SuppliersMarkup"] = DbSession.Query<Markup>()
				.Where(m => m.RegionId == region.Id && m.Type == 0).ToList();
			PropertyBag["DrugstoreMarkup"] = DbSession.Query<Markup>()
				.Where(m => m.RegionId == region.Id && m.Type == 1).ToList();
		}

		public void ShowRegions(uint? clientId, ulong? homeRegionId)
		{
			ShowRegions(clientId, homeRegionId, true, false);
		}

		public void ShowRegions(uint? clientId, ulong? homeRegionId, bool showDefaultRegions, bool showNonDefaultRegions)
		{
			var allRegions = Region.FindAll();
			if (homeRegionId.HasValue) {
				var homeRegion = Region.Find(homeRegionId.Value);
				if (showDefaultRegions)
					PropertyBag["defaultRegions"] = allRegions.Where(region => (region.Id & homeRegion.DefaultShowRegionMask) > 0);
				if (showNonDefaultRegions)
					PropertyBag["nonDefaultRegions"] = allRegions.Where(region => (region.Id & homeRegion.DefaultShowRegionMask) <= 0);
				PropertyBag["homeRegion"] = Region.Find(homeRegionId);
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
			var homeRegion = Region.Find(homeRegionId);
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
			var homeRegion = Region.Find(homeRegionId);
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