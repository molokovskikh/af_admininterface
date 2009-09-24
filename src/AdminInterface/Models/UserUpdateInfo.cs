using Castle.ActiveRecord;
using Castle.ActiveRecord.Linq;

namespace AdminInterface.Models
{
	//пока используется только в тестах, заготовка на будущее
	[ActiveRecord("usersettings.UserUpdateInfo")]
	public class UserUpdateInfo : ActiveRecordLinqBase<UserUpdateInfo>
	{
		[PrimaryKey("UserId")]
		public uint Id { get; set; }

		[Property]
		public string AFCopyId { get; set; }

		[Property]
		public uint AFAppVersion { get; set; }

		[BelongsTo("UserId")]
		public User User { get; set; }
	}
}
