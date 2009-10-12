using System;
using System.Linq;
using AdminInterface.Models.Security;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Linq;
using Common.Tools;

namespace AdminInterface.Models
{
	[ActiveRecord(Table = "`users`", Schema = "redmine")]
	public class RedmineUser : ActiveRecordLinqBase<RedmineUser>
	{
		public RedmineUser() {}

		public RedmineUser(Administrator admin)
		{
			var nameParts = admin.ManagerName.Split(' ');
			Login = admin.UserName;
			Mail = admin.Email;
			FirstName = nameParts.Skip(1).Implode(" ");
			LastName =  nameParts.First();
			Language = "ru";
			AuthSourceId = 1;
			CreatedOn = DateTime.Now;
			Status = 1;
		}

		[PrimaryKey]
		public uint Id { get; set; }
		[Property("login")]
		public string Login { get; set; }
		[Property("firstname")]
		public string FirstName { get; set; }
		[Property("lastname")]
		public string LastName { get; set; }
		[Property("mail")]
		public string Mail { get; set; }
		[Property("language")]
		public string Language { get; set; }
		[Property("auth_source_id")]
		public uint AuthSourceId { get; set; }
		[Property("created_on")]
		public DateTime CreatedOn { get; set; }
		[Property]
		public int Status { get; set; }
	}
}
