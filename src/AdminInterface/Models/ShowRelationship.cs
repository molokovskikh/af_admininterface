using Castle.ActiveRecord;

namespace AdminInterface.Models
{
	[ActiveRecord("UserSettings.showregulation")]
	public class ShowRelationship
	{
		[PrimaryKey]
		public uint Id { get; set; }

		[BelongsTo("PrimaryClientCode")]
		public Client Parent { get; set; }
	}
}