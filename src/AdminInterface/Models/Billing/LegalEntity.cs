using System;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.ActiveRecord.Linq;

namespace AdminInterface.Models.Billing
{
	[ActiveRecord(Schema = "Billing", Lazy = true)]
	public class LegalEntity : ActiveRecordLinqBase<LegalEntity>
	{
		public LegalEntity()
		{
		}

		public LegalEntity(string name, string fullName, Payer payer)
		{
			Name = name;
			FullName = fullName;
			Payer = payer;
		}

		public LegalEntity(string name, Payer payer)
			: this(name, name, payer)
		{
		}

		[PrimaryKey]
		public virtual uint Id { get; set; }

		[Property]
		public virtual string Name { get; set; }

		[Property]
		public virtual string FullName { get; set; }

		[BelongsTo("PayerId")]
		public virtual Payer Payer { get; set; }

		public override string ToString()
		{
			return Name;
		}
	}
}