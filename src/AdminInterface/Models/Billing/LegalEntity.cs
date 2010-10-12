using Castle.ActiveRecord;
using Castle.ActiveRecord.Linq;

namespace AdminInterface.Models.Billing
{
	[ActiveRecord("LegalEntities", Schema = "Billing", Lazy = true)]
	public class LegalEntity : ActiveRecordLinqBase<LegalEntity>
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

		[BelongsTo("RecipientId")]
		public virtual Recipient Recipient { get; set; }
	}
}