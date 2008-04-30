using System;
using Castle.ActiveRecord;

namespace AdminInterface.Models.Logs
{
	[ActiveRecord(Table = "logs.clientsinfo")]
	public class ClientInfoLogEntity : ActiveRecordBase<ClientInfoLogEntity>
	{
		[PrimaryKey("RowId")]
		public uint Id { get; set; }
		[Property]
		public string UserName { get; set; }
		[Property]
		public string Message { get; set; }
		[Property]
		public uint ClientCode { get; set; }
		[Property]
		public DateTime WriteTime { get; set; }

		public ClientInfoLogEntity SetProblem(bool isFree, string problem)
		{
			if (isFree)
				Message = "$$$Бесплатное изменение пароля: " + problem;
			else
				Message = "$$$Платное изменение пароля: " + problem;
			return this;
		}
	}
}
