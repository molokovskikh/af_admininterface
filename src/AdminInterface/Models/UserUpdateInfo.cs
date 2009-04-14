using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Castle.ActiveRecord;

namespace AdminInterface.Models
{
	//пока используется только в тестах, заготовка на будущее
	[ActiveRecord("usersettings.UserUpdateInfo")]
	public class UserUpdateInfo : ActiveRecordBase<UserUpdateInfo>
	{
		[PrimaryKey("UserId")]
		public uint Id { get; set; }

		[Property]
		public string AFCopyId { get; set; }

		[BelongsTo("UserId")]
		public User User { get; set; }
	}
}
