using System;
using System.Linq;
using Castle.ActiveRecord;
using Common.Web.Ui.Models;
using NHibernate;
using NHibernate.Linq;

namespace AdminInterface.Models.Suppliers
{
	[ActiveRecord("RegionalData", Schema = "Usersettings")]
	public class RegionalData
	{
		public RegionalData()
		{
			ContactInfo = "";
			OperativeInfo = "";
		}

		public RegionalData(Region region, Supplier supplier)
			: this()
		{
			Region = region;
			Supplier = supplier;
		}

		[PrimaryKey("RowId")]
		public virtual uint Id { get; set; }

		[BelongsTo("RegionCode")]
		public virtual Region Region { get; set; }

		[BelongsTo("FirmCode")]
		public virtual Supplier Supplier { get; set; }

		[Property(NotNull = true)]
		public virtual string ContactInfo { get; set; }

		[Property(NotNull = true)]
		public virtual string OperativeInfo { get; set; }

		public static void AddForSuppler(ISession dbSession, uint supplierId, ulong newMask)
		{
			var supplier = dbSession.Get<Supplier>(supplierId);
			var supplierRegions = dbSession.Query<Region>().Where(r => (r.Id & newMask) > 0).ToList();
			var supplierRegionalData = dbSession.Query<RegionalData>().Where(r => r.Supplier == supplier).Select(r => r.Region).ToList();
			var newRegions = supplierRegions.Where(n => !supplierRegionalData.Contains(n)).ToList();
			foreach (var newRegion in newRegions) {
				var regionalData = new RegionalData(newRegion, supplier);
				dbSession.Save(regionalData);
				foreach (var value in Enum.GetValues(typeof(DayOfWeek))) {
					var day = (DayOfWeek)value;
					var reorderSchedule = new ReorderSchedule(regionalData, day);
					reorderSchedule.TimeOfStopsOrders = TimeSpan.FromTicks(684000000000);
					if (day == DayOfWeek.Saturday)
						reorderSchedule.TimeOfStopsOrders = TimeSpan.FromTicks(504000000000);
					if (day == DayOfWeek.Sunday)
						reorderSchedule.TimeOfStopsOrders = TimeSpan.FromTicks(863400000000);
					dbSession.Save(reorderSchedule);
				}
			}
		}
	}
}