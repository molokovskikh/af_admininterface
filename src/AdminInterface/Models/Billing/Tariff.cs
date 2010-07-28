using Castle.ActiveRecord;

namespace AdminInterface.Models.Billing
{
	[ActiveRecord("Tariffs", Schema = "Billing")]
	public class Tariff : ActiveRecordBase<Tariff>
	{
		[PrimaryKey]
		public uint Id { get; set; }

		[BelongsTo(Column = "RegionCode")]
		public Region Region { get; set; }

		[Property]
		public float Pay { get; set; }
	}
}
