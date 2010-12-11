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
			var defaults = DefaultValues.Get();
			AFAppVersion = defaults.AnalitFVersion;
			TargetVersion = defaults.AnalitFVersion;
			AFCopyId = "";
		}

		public UserUpdateInfo() { }

		[PrimaryKey("UserId", Generator = PrimaryKeyType.Assigned)]
		public virtual uint Id { get; set; }

		[Property]
		public virtual string AFCopyId { get; set; }

		[Property]
		public virtual uint AFAppVersion { get; set; }

		[Property]
		public virtual uint? TargetVersion { get; set; }
	}
}
