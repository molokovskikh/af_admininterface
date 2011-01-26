using System.Collections.Generic;
using System.Linq;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Linq;

namespace AdminInterface.Models.Billing
{
	[ActiveRecord(Schema = "Billing")]
	public class Recipient : ActiveRecordLinqBase<Recipient>
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

		public static IList<Recipient> All()
		{
			return Queryable.OrderBy(r => r.Name).ToList();
		}
	}
}