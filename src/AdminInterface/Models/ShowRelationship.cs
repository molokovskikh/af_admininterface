using Castle.ActiveRecord;

namespace AdminInterface.Models
{
	[ActiveRecord("showregulation", Schema = "UserSettings")]
	public class ShowRelationship : ActiveRecordBase<ShowRelationship>
	{
		public ShowRelationship()
		{}

		public ShowRelationship(Client parent, Client child)
		{
			Parent = parent;
			Child = child;
		}

		[PrimaryKey]
		public uint Id { get; set; }

		[BelongsTo("PrimaryClientCode")]
		public Client Parent { get; set; }

		[BelongsTo("ShowClientCode")]
		public Client Child { get; set; }
	}
}