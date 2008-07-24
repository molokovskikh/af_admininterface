using System.ComponentModel;
using Castle.ActiveRecord;

namespace AdminInterface.Models
{
	public enum RelationshipType
	{
		[Description("�������")] Base = 0,
		[Description("�������+")] BasePlus = 3,
		[Description("�������")] Hidden = 2,
		[Description("����")] Network = 1,
	}

	[ActiveRecord("UserSettings.includeregulation")]
	public class Relationship
	{
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