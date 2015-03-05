using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;

namespace AdminInterface.Models
{
	[ActiveRecord(Schema = "UserSettings")]
	public class CostOptimizationForbiddenClient
	{
		public CostOptimizationForbiddenClient()
		{
		}

		public CostOptimizationForbiddenClient(Client client)
		{
			Client = client;
		}

		[PrimaryKey]
		public virtual uint Id { get; set; }

		[BelongsTo("ClientId")]
		public virtual Client Client { get; set; }
	}

	[ActiveRecord(Schema = "UserSettings")]
	public class CostOptimizationForbiddenConcurrent
	{
		public CostOptimizationForbiddenConcurrent()
		{
		}

		public CostOptimizationForbiddenConcurrent(Supplier supplier)
		{
			Supplier = supplier;
		}

		[PrimaryKey]
		public virtual uint Id { get; set; }

		[BelongsTo("SupplierId")]
		public virtual Supplier Supplier { get; set; }
	}
}