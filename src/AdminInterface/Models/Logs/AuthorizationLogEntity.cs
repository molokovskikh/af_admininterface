using System;
using Castle.ActiveRecord;

namespace AdminInterface.Models.Logs
{
	[ActiveRecord(Table = "logs.AuthorizationDates")]
	public class AuthorizationLogEntity : ActiveRecordBase<AuthorizationLogEntity>
	{
		[PrimaryKey("UserId", Generator = PrimaryKeyType.Assigned)]
		public uint Id { get; set; }

		[Property]
		public DateTime? CITime { get; set; }

		[Property]
		public DateTime? AFTime { get; set; }

		[Property]
		public DateTime? AOLTime { get; set; }

		[Property]
		public DateTime? IOLTime { get; set; }
	}
}
