using System.Linq;
using Castle.ActiveRecord;
using Castle.Components.Validator;

namespace AdminInterface.Models
{
	[ActiveRecord(Table = "UserUpdateInfo", Schema = "usersettings")]
	public class UserMessage : ActiveRecordValidationBase<UserMessage>
	{
		[PrimaryKey("UserId")]
		public uint Id { get; set; }

		[BelongsTo("UserId", Insert = false)]
		public User User { get; set; }

		[Property, ValidateNonEmpty("Нужно ввести текст сообщения")]
		public string Message { get; set; }

		[Property("MessageShowCount")]
		public uint ShowMessageCount { get; set; }

		public bool IsContainsNotShowedMessage()
		{
			return ShowMessageCount > 0;
		}

		public static UserMessage FindUserMessage(uint userId)
		{
			return TryFind(userId);
		}
	}
}