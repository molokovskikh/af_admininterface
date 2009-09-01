using System.ComponentModel;
using Castle.ActiveRecord;

namespace AdminInterface.Models
{
	public enum RelationshipType
	{
		[Description("Базовый")] Base = 0,
		[Description("Базовый+")] BasePlus = 3,
		[Description("Скрытый")] Hidden = 2,
		[Description("Сеть")] Network = 1,
	}

	[ActiveRecord("UserSettings.includeregulation")]
	public class Relationship : ActiveRecordBase
	{
		public Relationship()
		{}

		public Relationship(Client parent, Client child, RelationshipType relationshipType)
		{
			Parent = parent;
			Child = child;
			RelationshipType = relationshipType;
		}

		[PrimaryKey]
		public uint Id { get; set; }

		[BelongsTo("PrimaryClientCode")]
		public Client Parent { get; set; }

		[BelongsTo("IncludeClientCode")]
		public Client Child { get; set; }

		[Property("IncludeType")]
		public RelationshipType RelationshipType { get; set; }
	}
}