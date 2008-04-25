using Castle.ActiveRecord;
using Castle.Components.Validator;

namespace AdminInterface.Models
{
	[ActiveRecord(Table = "Usersettings.retclientsset")]
	public class ClientMessage : ActiveRecordValidationBase<ClientMessage>
	{
		[Property]
		public string Message { get; set; }

		[Property]
		public uint ShowMessageCount { get; set; }

		[PrimaryKey]
		public uint ClientCode { get; set; }

		public bool IsContainsNotShowedMessage()
		{
			return ShowMessageCount > 0;
		}
	}
}