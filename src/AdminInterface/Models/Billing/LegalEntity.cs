using Castle.ActiveRecord;
using Castle.ActiveRecord.Linq;

namespace AdminInterface.Models.Billing
{
	[ActiveRecord(Schema = "Billing", Lazy = true)]
	public class LegalEntity : ActiveRecordLinqBase<LegalEntity>
	{
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