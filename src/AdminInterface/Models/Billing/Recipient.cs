using System.Collections.Generic;
using System.Linq;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.ActiveRecord.Linq;
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

		public string Description
		{
			get
			{
				if (Id == 4)
					return "Обеспечение доступа к ИС (мониторингу фармрынка) в {0}";
				return "Мониторинг оптового фармрынка за {0}";
			}
		}

		public static IList<Recipient> All()
		{
			return ArHelper.WithSession(s => All(s));
		}

		public static IList<Recipient> All(ISession session)
		{
			return session.Query<Recipient>().OrderBy(r => r.Name).ToList();
		}
	}
}
