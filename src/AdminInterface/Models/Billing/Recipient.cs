using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.ActiveRecord.Linq;
using Castle.Components.Validator;
using Common.Web.Ui.ActiveRecordExtentions;
using NHibernate;
using NHibernate.Linq;

namespace AdminInterface.Models.Billing
{
	[ActiveRecord(Schema = "Billing")]
	public class Recipient
	{
		[PrimaryKey]
		public virtual uint Id { get; set; }

		[Property]
		public virtual string Name { get; set; }

		[Property]
		public virtual string FullName { get; set; }

		[Property]
		public virtual string Address { get; set; }

		[Property]
		public virtual string INN { get; set; }

		[Property]
		public virtual string KPP { get; set; }

		[Property]
		public virtual string BIC { get; set; }

		[Property]
		public virtual string Bank { get; set; }

		[Property]
		public virtual string BankLoroAccount { get; set; }

		[Property]
		public virtual string BankAccountNumber { get; set; }

		[Property]
		public virtual string Boss { get; set; }

		[Property]
		public virtual string Accountant { get; set; }

		[Property]
		public virtual string AccountWarranty { get; set; }

		[Property, ValidateNonEmpty, Description("Наименование работы (услуги) для пользователя:")]
		public virtual string UserDescription { get; set; }

		[Property, ValidateNonEmpty, Description("Наименование работы (услуги) для адреса:")]
		public virtual string AddressDescription { get; set; }

		[Property, ValidateNonEmpty, Description("Наименование работы (услуги) для отчета:")]
		public virtual string ReportDescription { get; set; }

		[Property, ValidateNonEmpty, Description("Наименование работы (услуги) для поставщика:")]
		public virtual string SupplierDescription { get; set; }

		public static IList<Recipient> All()
		{
			return ArHelper.WithSession(s => All(s));
		}

		public static IList<Recipient> All(ISession session)
		{
			return session.Query<Recipient>().OrderBy(r => r.Name).ToList();
		}

		public static Recipient CreateWithDefaults()
		{
			return new Recipient {
				UserDescription = "Мониторинг оптового фармрынка за {0}",
				AddressDescription = "Дополнительный адрес доставки медикаментов за {0}",
				ReportDescription = "Статистический отчет по фармрынку за {0}",
				SupplierDescription = "Справочно-информационные услуги за {0}"
			};
		}
	}
}