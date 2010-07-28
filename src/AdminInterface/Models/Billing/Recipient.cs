using Castle.ActiveRecord;
using Castle.ActiveRecord.Linq;

namespace AdminInterface.Models.Billing
{
	[ActiveRecord("Recipients", Schema = "Billing")]
	public class Recipient : ActiveRecordLinqBase<Recipient>
	{
		[PrimaryKey]
		public virtual uint Id { get; set; }

		[Property]
		public virtual string Name { get; set; }
	}
}