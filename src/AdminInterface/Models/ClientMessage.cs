using Castle.ActiveRecord;
using Castle.Components.Validator;

namespace AdminInterface.Models
{
	[ActiveRecord(Table = "Usersettings.retclientsset")]
	public class ClientMessage : ActiveRecordValidationBase<ClientMessage>
	{
		[PrimaryKey("ClientCode")]
		public uint ClientCode { get; set; }

		[Property, ValidateNonEmpty("Нужно ввести текст сообщения")]
		public string Message { get; set; }

		[Property]
		public uint ShowMessageCount { get; set; }

		public bool IsContainsNotShowedMessage()
		{
			return ShowMessageCount > 0;
		}
	}
}