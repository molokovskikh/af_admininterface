using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Suppliers;
using AdminInterface.MonoRailExtentions;
using AdminInterface.Queries;
using AdminInterface.Security;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Helpers;
using NHibernate.SqlCommand;
using RemotePriceProcessor;

namespace AdminInterface.Controllers
{
	public class InboundPriceItems
	{
		public InboundPriceItems(bool down, Price price, string filePath, DateTime priceTime, int hash, bool formalizedNow)
		{
			Downloaded = down;
			if (price != null) {
				Supplier = price.Supplier;
				SupplierCode = price.Supplier.Id;
				SupplierName = price.Supplier.Name;
				PriceCode = price.Id;
				PriceName = price.Name;
			}
			Extension = Path.GetExtension(Path.GetFileName(filePath)).Replace(".", string.Empty);
			PriceTime = priceTime;
			Hash = hash;
			FormalizedNow = formalizedNow;
		}

		public bool Downloaded { get; set; }
		public Supplier Supplier { get; set; }
		public uint SupplierCode { get; set; }
		public string SupplierName { get; set; }
		public uint PriceCode { get; set; }
		public string PriceName { get; set; }
		public string Extension { get; set; }
		public DateTime PriceTime { get; set; }
		public int Hash { get; set; }
		public bool FormalizedNow { get; set; }
	}

	[
		Secure,
		Helper(typeof(ViewHelper)),
		Helper(typeof(BindingHelper)),
		Filter(ExecuteWhen.BeforeAction, typeof(SecurityActivationFilter))
	]
	public class MonitoringController : AdminInterfaceController
	{
		public void Updates()
		{
			var sortMap = new Dictionary<string, string> {
				{ "MethodName", "MethodName" },
				{ "StartTime", "StartTime" },
				{ "ShortName", "c.Name" },
				{ "ClientCode", "c.Id" },
				{ "User", "u.Name" }
			};

			var sortable = new Sortable(sortMap);
			BindObjectInstance(sortable, "filter");

			var criteria = DbSession.CreateCriteria<PrgDataLog>()
				.CreateAlias("User", "u", JoinType.InnerJoin)
				.CreateAlias("u.Client", "c", JoinType.InnerJoin);
			sortable.ApplySort(criteria);
			var logs = criteria.List<PrgDataLog>();

			PropertyBag["logs"] = logs;
			PropertyBag["filter"] = sortable;
		}

		public void Orders()
		{
			PropertyBag["Orders"] = new OrderFilter { NotSent = true }.Find();
		}

		public void InboundPriceItemsList()
		{
			var items = new WcfPriceProcessItem[0];
#if !DEBUG
			RemoteServiceHelper.RemotingCall(s => {
				items = s.GetPriceItemList();
			});
#else
			items = new[] {
				new WcfPriceProcessItem(0, false, "jjj.AAA", 0, null, DateTime.Now.AddMinutes(50), 0),
				new WcfPriceProcessItem(0, true, "jjj.123", 0, null, DateTime.Now.AddMinutes(10), 0),
				new WcfPriceProcessItem(0, false, "jjj.BBB", 0, null, DateTime.Now.AddMinutes(100), 0) { FormalizedNow = true },
				new WcfPriceProcessItem(0, true, "jjj.789", 0, null, DateTime.Now.AddMinutes(500), 0)
			};
#endif
			var result = items.Select(i => {
				var price = DbSession.Get<Price>((uint)i.PriceCode);
				return new InboundPriceItems(i.Downloaded, price, i.FilePath, i.CreateTime, i.HashCode, i.FormalizedNow);
			}).ToList();

			var sortable = new Sortable();
			BindObjectInstance(sortable, "filter");

			var sortBy = sortable.SortBy;
			var direction = sortable.SortDirection == "asc" ? "ascending" : "descending";
			if (string.IsNullOrEmpty(sortable.SortDirection))
				direction = "ascending";
			var retranse = result.Where(r => !r.Downloaded).Sort(ref sortBy, ref direction, "PriceTime").ToList();
			var form = result.Where(r => r.Downloaded).Sort(ref sortBy, ref direction, "PriceTime").ToList();
			form.AddRange(retranse);

			PropertyBag["items"] = form.ToList();
			PropertyBag["filter"] = sortable;
		}

		public void DeleteItemInInboundList(int hashCode)
		{
			var result = false;
			RemoteServiceHelper.RemotingCall(s => {
				result = s.DeleteItemInInboundList(hashCode);
			});
			if (result)
				Notify("Прайс лист удален из очереди");
			else
				Error("Ошибка при удалении прайс листа");
			RedirectToReferrer();
		}

		public void TopInInboundList(int hashCode)
		{
			var result = false;
			RemoteServiceHelper.RemotingCall(s => {
				result = s.TopInInboundList(hashCode);
			});
			if (result)
				Notify("Прайс лист перемещен вверх");
			else
				Error("Ошибка при перемещении прайс листа");
			RedirectToReferrer();
		}
	}
}