﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Suppliers;
using AdminInterface.MonoRailExtentions;
using AdminInterface.Queries;
using AdminInterface.Security;
using Castle.MonoRail.Framework;
using Common.Tools;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using NHibernate.Linq;
using NHibernate.SqlCommand;
using RemotePriceProcessor;

namespace AdminInterface.Controllers
{
	public class InboundPriceItems
	{
		public InboundPriceItems(bool down, Price price, string filePath, DateTime? priceTime, int hash, bool formalizedNow)
		{
			Downloaded = down;
			if (price != null) {
				Supplier = price.Supplier;
				SupplierCode = price.Supplier.Id;
				SupplierName = price.Supplier.Name;
				SupplierRegion = price.Supplier.HomeRegion.Name;
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
		public string SupplierRegion { get; set; }
		public uint PriceCode { get; set; }
		public string PriceName { get; set; }
		public string Extension { get; set; }
		public DateTime? PriceTime { get; set; }
		public int Hash { get; set; }
		public bool FormalizedNow { get; set; }
		public bool Error { get; set; }
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
			PropertyBag["IsMonitoring"] = true;
		}

		public static uint ParseId(string filename)
		{
			var name = Path.GetFileNameWithoutExtension(filename);

			if (String.IsNullOrEmpty(name))
				return 0;

			var matches = new[] {
				Regex.Match(name, @"^(\d+)$"),
				Regex.Match(name, @"^d(\d+)_")
			};

			var match = matches.FirstOrDefault(m => m.Success);
			if (match != null) {
				return SafeConvert.ToUInt32(match.Groups[1].Value);
			}
			return 0;
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
				new WcfPriceProcessItem(1, false, "jjj.AAA", 2983, null, DateTime.Now.AddMinutes(50), 0),
				new WcfPriceProcessItem(2, true, "jjj.123", 384, null, DateTime.Now.AddMinutes(10), 0),
				new WcfPriceProcessItem(3, false, "jjj.BBB", 0, null, DateTime.Now.AddMinutes(100), 0) { FormalizedNow = true },
				new WcfPriceProcessItem(4, true, "jjj.789", 1264, null, DateTime.Now.AddMinutes(500), 0)
			};
#endif
			var codes = items.Select(i => Convert.ToUInt32(i.PriceCode)).ToList();
			var prices = DbSession.Query<Price>().Where(p => codes.Contains(p.Id)).ToList().ToDictionary(k => k.Id);
			var itemsCodes = items.Select(i => (uint)i.PriceItemId).ToList();
			var priceItems = DbSession.Query<PriceItem>().Where(i => itemsCodes.Contains(i.Id)).ToList().ToDictionary(k => k.Id);

#if DEBUG
			prices = new Dictionary<uint, Price>() { { 1u, new Price { Supplier = new Supplier { HomeRegion = new Region("test") } } }, { 4, new Price() { Supplier = new Supplier { HomeRegion = new Region("test2") } } } };
#endif

			var result = items.Select(i => {
				if (prices.Keys.Contains((uint)i.PriceCode)) {
					var price = prices[(uint)i.PriceCode];
					DateTime? log = null;
					if (priceItems.Keys.Contains((uint)i.PriceItemId) && i.Downloaded) {
						var logs = priceItems[(uint)i.PriceItemId].DownloadLogs;
						if (logs.Count > 0)
							log = logs.Max(d => d.LogTime);
					}
					return new InboundPriceItems(i.Downloaded, price, i.FilePath, log ?? (i.FileTime ?? i.CreateTime), i.HashCode, i.FormalizedNow);
				}
				return null;
			}).Where(p => p != null).ToList();

			if (Directory.Exists(Config.ErrorFilesPath)) {
				var errorsItems = Directory.GetFiles(Config.ErrorFilesPath).Select(f => new { name = Path.GetFileNameWithoutExtension(f), time = File.GetCreationTime(f), file = f, priceItemId = ParseId(Path.GetFileNameWithoutExtension(f)) });
				var itemIds = errorsItems.Select(e => e.priceItemId).ToList();
				var errorPrices = DbSession.Query<Cost>().Where(p => itemIds.Contains(p.PriceItem.Id)).Select(p => p.Price).Distinct().ToList();
				result.AddRange(errorsItems.Select(e => {
					var price = errorPrices.FirstOrDefault(p => p.Costs.Select(c => c.PriceItem.Id).Contains(e.priceItemId));
					if (price != null)
						return new InboundPriceItems(false, price, e.file, e.time, 0, false) { Error = true };
					return null;
				}));
			}

			result = result.Where(r => r != null).ToList();

			var sortable = new Sortable();
			BindObjectInstance(sortable, "filter");

			var sortBy = sortable.SortBy;
			var direction = sortable.SortDirection == "asc" ? "ascending" : "descending";
			if (string.IsNullOrEmpty(sortable.SortDirection))
				direction = "ascending";
			var retranse = result.Where(r => !r.Downloaded).Sort(ref sortBy, ref direction, "PriceTime").ToList();
			var form = result.Where(r => r.Downloaded).Sort(ref sortBy, ref direction, "PriceTime").ToList();
			form.AddRange(retranse);
			form = form.OrderBy(f => f.Error).ToList();

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