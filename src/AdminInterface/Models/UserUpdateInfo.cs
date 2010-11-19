using Castle.ActiveRecord;
using Castle.ActiveRecord.Linq;

namespace AdminInterface.Models
{
	[ActiveRecord("UserUpdateInfo", Schema = "usersettings")]
	public class UserUpdateInfo : ActiveRecordLinqBase<UserUpdateInfo>
	{
		public UserUpdateInfo(uint id)
		{
			Id = id;
			AFAppVersion = DefaultValues.Get().AnalitFVersion;
			AFCopyId = "";
		}

		public UserUpdateInfo() { }

		[PrimaryKey("UserId", Generator = PrimaryKeyType.Assigned)]
		public uint Id { get; set; }

		[Property]
		public string AFCopyId { get; set; }

		[Property]
		public uint AFAppVersion { get; set; }
	}
}
