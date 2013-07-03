using System;
using Castle.ActiveRecord;

namespace AdminInterface.Models.Telephony
{
	[ActiveRecord("CallbackPhones", Schema = "telephony")]
	public class Callback
	{
		[PrimaryKey]
		public uint Id { get; set; }

		[Property]
		public string CallerPhone { get; set; }

		[Property]
		public bool Enabled { get; set; }

		[Property]
		public DateTime? DueDate { get; set; }

		[Property]
		public bool CheckDate { get; set; }

		[Property]
		public string Comment { get; set; }
	}
}