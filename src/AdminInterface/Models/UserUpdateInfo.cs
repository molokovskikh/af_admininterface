using System;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.ActiveRecord.Linq;

namespace AdminInterface.Models
{
	[ActiveRecord("UserUpdateInfo", Schema = "usersettings")]
	public class UserUpdateInfo : ActiveRecordLinqBase<UserUpdateInfo>
	{
		public UserUpdateInfo(User user)
		{
			User = user;
			AFCopyId = "";
		}

		public UserUpdateInfo()
		{
		}

		[PrimaryKey("UserId", Generator = PrimaryKeyType.Foreign)]
		public virtual uint Id { get; set; }

		[OneToOne]
		public virtual User User { get; set; }

		[Property]
		public virtual string AFCopyId { get; set; }

		[Property]
		public virtual uint AFAppVersion { get; set; }
	}
}