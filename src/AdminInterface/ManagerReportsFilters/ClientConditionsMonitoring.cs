using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdminInterface.Models.Suppliers;
using Common.Web.Ui.Helpers;
using NHibernate;
using NHibernate.Linq;

namespace AdminInterface.ManagerReportsFilters
{
	public class MonitoringItem
	{
		public MonitoringItem(Intersection item)
		{
			SupplierCode = item.Price.Supplier.Id;
			SupplierName = item.Price.Supplier.Name;
			PriceName = item.Price.Name;
			CostName = item.Cost.Name;
			PriceMarkup = item.PriceMarkup;
			SupplierClientId = item.SupplierClientId;
			SupplierPaymentId = item.SupplierPaymentId;
			SupplierDeliveryId = String.Join(",", item.Addresses.Select(a => a.SupplierDeliveryId));

			Cost = item.Cost;
		}

		public uint SupplierCode { get; set; }
		public string SupplierName { get; set; }
		public string PriceName { get; set; }
		public string CostName { get; set; }
		public double PriceMarkup { get; set; }
		public string SupplierPaymentId { get; set; }
		public string SupplierClientId { get; set; }
		public string SupplierDeliveryId { get; set; }

		protected Cost Cost { get; set; }

		[Style]
		public bool CostCollumn
		{
			get { return  }
		}
	}

	public class ClientConditionsMonitoring : PaginableSortable
	{
		public ISession Session;
		public uint ClientId { get; set; }

		public IList<MonitoringItem> Find()
		{
			Session.Query<Intersection>()
				.Where(i => i.Client.Id == ClientId)
				.Select(i => new MonitoringItem(i));
		}
	}
}