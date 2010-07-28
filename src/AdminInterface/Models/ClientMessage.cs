using System.Linq;
using Castle.ActiveRecord;
using Castle.Components.Validator;

namespace AdminInterface.Models
{
	[ActiveRecord(Table = "UserUpdateInfo", Schema = "usersettings")]
	public class ClientMessage : ActiveRecordValidationBase<ClientMessage>
	{
		[PrimaryKey("UserId")]
		public uint ClientCode { get; set; }

		[Property, ValidateNonEmpty("Нужно ввести текст сообщения")]
		public string Message { get; set; }

		[Property("MessageShowCount")]
		public uint ShowMessageCount { get; set; }

		public bool IsContainsNotShowedMessage()
		{
			return ShowMessageCount > 0;
		}

		public static ClientMessage FindClientMessage(uint clientCode)
		{
			var client = Client.Find(clientCode);
			var user = client.Users.FirstOrDefault();
			if (user == null)
				return null;
			return TryFind((user.Id));
		}

		public static ClientMessage FindUserMessage(uint userId)
		{
			return TryFind(userId);
		}
	}
}