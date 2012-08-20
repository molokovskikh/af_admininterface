using System.ComponentModel;
using System.Linq;
using AdminInterface.Models.Billing;
using Castle.ActiveRecord;
using Castle.Components.Validator;
using Common.Tools;
using Common.Web.Ui.Models;

namespace AdminInterface.Models
{
	[ActiveRecord(Table = "UserUpdateInfo", Schema = "usersettings")]
	public class UserMessage : ActiveRecordValidationBase<UserMessage>
	{
		[PrimaryKey("UserId")]
		public uint Id { get; set; }

		[BelongsTo("UserId", Insert = false)]
		public User User { get; set; }

		public Payer Payer { get; set; }

		[Property, ValidateNonEmpty("Нужно ввести текст сообщения")]
		public string Message { get; set; }

		[Property("MessageShowCount")]
		public uint ShowMessageCount { get; set; }

		[Description("Тема письма:")]
		public string Subject { get; set; }

		[Description("Отправлять это сообщение также на email")]
		public bool SendToEmail { get; set; }

		[Description("Отправлять это сообщение также в минипочту")]
		public bool SendToMinimail { get; set; }

		public bool Mail
		{
			get { return SendToEmail || SendToMinimail; }
		}

		public string To
		{
			get
			{
				if (User != null)
					return User.GetEmailForBilling();

				var mails = Enumerable.Empty<string>();
				if (SendToEmail) {
					mails = mails
						.Concat(Payer.Clients
							.SelectMany(c => c.ContactGroupOwner.GetEmails(ContactGroupType.Billing)));

					mails = mails
						.Concat(Payer.ContactGroupOwner
							.GetEmails(ContactGroupType.Billing));
				}

				if (SendToMinimail)
					mails = mails.Concat(Payer.ClientsMinimailAddresses);

				return mails
					.Distinct()
					.Implode();
			}
		}

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