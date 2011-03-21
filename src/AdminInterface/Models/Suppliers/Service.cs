using Castle.ActiveRecord;

namespace AdminInterface.Models.Suppliers
{
	[ActiveRecord(Schema = "Future"), JoinedBase]
	public class Service
	{
		[PrimaryKey]
		public virtual uint Id { get; set; }
	}
}