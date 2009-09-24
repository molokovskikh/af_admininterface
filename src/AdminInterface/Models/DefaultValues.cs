using System.Linq;
using Castle.ActiveRecord;

namespace AdminInterface.Models
{
	[ActiveRecord(Table = "UserSettings.Defaults")]
	public class DefaultValues : ActiveRecordBase<DefaultValues>
	{
		[PrimaryKey]
		public uint AnalitFVersion { get; set; }

		public static DefaultValues Get()
		{
			return FindAll().First();
		}
	}
}
