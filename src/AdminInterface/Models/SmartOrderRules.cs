using System.Collections.Generic;
using System.ComponentModel;
using AdminInterface.Models.Audit;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Common.Web.Ui.Models.Audit;
using System.Linq;

namespace AdminInterface.Models
{
	[ActiveRecord("smart_order_rules", Schema = "ordersendrules", Lazy = true), Auditable]
	public class SmartOrderRules : ActiveRecordLinqBase<SmartOrderRules>, IMultiAuditable
	{
		[PrimaryKey("Id")]
		public virtual uint Id { get; set; }

		[Property, Description("Парсер для автозаказа"), Auditable]
		public virtual string ParseAlgorithm { get; set; }

		[BelongsTo, Description("Ассортиментный прайс лист для автозаказа"), Auditable]
		public virtual Price AssortimentPriceCode { get; set; }

		[HasMany]
		public virtual IList<DrugstoreSettings> Settings { get; set; }

		public static SmartOrderRules TestSmartOrder()
		{
			var testOrder = new SmartOrderRules {
#if !DEBUG
				AssortimentPriceCode = ActiveRecordMediator<Price>.FindByPrimaryKey(4662u),
#endif
				ParseAlgorithm = "TestSource",
			};
			return testOrder;
		}

		public virtual string GetAssortimentPriceName()
		{
			if (AssortimentPriceCode != null) {
				return string.Format("{0} - {1}", AssortimentPriceCode.Supplier.Name, AssortimentPriceCode.Name);
			}
			return string.Empty;
		}

		public virtual IEnumerable<IAuditRecord> GetAuditRecords(IEnumerable<AuditableProperty> properties = null)
		{
			return Settings.Where(s => s.Client != null).Select(s => new AuditRecord(s));
		}
	}
}