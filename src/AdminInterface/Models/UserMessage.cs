using System.Linq;
using Castle.ActiveRecord;
using Castle.Components.Validator;

namespace AdminInterface.Models
{
	[ActiveRecord(Table = "UserUpdateInfo", Schema = "usersettings")]
	public class UserMessage : ActiveRecordValidationBase<UserMessage>
	{
		[PrimaryKey("UserId")]
		public uint ClientCode { get; set; }

		[Property, ValidateNonEmpty("����� ������ ����� ���������")]
		public string Message { get; set; }

		[Property("MessageShowCount")]
		public uint ShowMessageCount { get; set; }

		public bool IsContainsNotShowedMessage()
		{
			return ShowMessageCount > 0;
		}

		public static UserMessage FindClientMessage(uint clientCode)
		{
			var client = Client.Find(clientCode);
			var user = client.Users.FirstOrDefault();
			if (user == null)
				return null;
			return TryFind((user.Id));
		}

		public static UserMessage FindUserMessage(uint userId)
		{
			return TryFind(userId);
		}
	}
}