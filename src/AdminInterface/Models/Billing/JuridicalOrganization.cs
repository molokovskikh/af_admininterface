using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Linq;

namespace AdminInterface.Models.Billing
{
	[ActiveRecord("JuridicalOrganizations", Schema = "Billing", Lazy = true)]
	public class JuridicalOrganization : ActiveRecordLinqBase<JuridicalOrganization>
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
		public virtual string ReceiverAddress { get; set; }

		[Property]
		public virtual string Inn { get; set; }

		[Property]
		public virtual string Kpp { get; set; }

		[BelongsTo("PayerId")]
		public virtual Payer Payer { get; set; }
	}
}