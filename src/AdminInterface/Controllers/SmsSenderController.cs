using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Security;
using AdminInterface.MonoRailExtentions;
using AdminInterface.Security;
using Castle.MonoRail.Framework;
using Common.Tools;
using System.Web;
using System.Linq;
using NHibernate.Linq;

namespace AdminInterface.Controllers
{
	public class SmsSenderController : AdminInterfaceController
	{
		public void Index()
		{
			PropertyBag["usersList"] = DbSession.Query<Administrator>().OrderBy(s=>s.ManagerName).ToList();
		}
		public void MessagesList(uint id)
		{
			var userTo = DbSession.Query<Administrator>().FirstOrDefault(s => s.Id == id);
			PropertyBag["recieverAdmin"] = userTo;
			PropertyBag["messagesList"] =
				DbSession.Query<InnerSmsMessage>().Where(s => s.UserTo.Id == id).OrderByDescending(s => s.Id).ToList();
		}

		[AccessibleThrough(Verb.Get)]
		public void Add(uint id)
		{
			var currentUserId = SecurityContext.Administrator.Id;
			var userFrom = DbSession.Query<Administrator>().FirstOrDefault(s => s.Id == currentUserId);
			var userTo = DbSession.Query<Administrator>().FirstOrDefault(s => s.Id == id);

			PropertyBag["messageData"] = new InnerSmsMessage()
			{
				Id = id,
				UserTo = userTo,
				UserFrom = userFrom,
				TargetAddress = userTo?.PhoneSupport?.Replace("-", ""),
				Message = ""
			};
		}

		[AccessibleThrough(Verb.Post)]
		public void Add(uint id, string address, string message)
		{
			var currentUserId = SecurityContext.Administrator.Id;
			var userFrom = DbSession.Query<Administrator>().FirstOrDefault(s => s.Id == currentUserId);
			var userTo = DbSession.Query<Administrator>().FirstOrDefault(s => s.Id == id);
			PropertyBag["messageData"] = new InnerSmsMessage() {
				Id = id,
				UserTo = userTo,
				UserFrom = userFrom,
				TargetAddress = address,
				Message = message
			};
			if (userFrom == null) {
				Error("не указан отправитель");
				return;
			}
			if (userTo == null) {
				Error("не указан получатель");
				return;
			}
			if (string.IsNullOrEmpty(address.Trim())) {
				Error("не указан номер");
				return;
			}
			address = address.Replace("-", "");
			if (address.Length != 10 || !address.All(char.IsDigit)) {
				Error($"неправильный формат номера: {address}. Введите 10-ти значный номер.");
				return;
			}
			if (string.IsNullOrEmpty(message.Trim())) {
				Error("не указан текст сообщения");
				return;
			}

			var error = "";
			int smsId = Func.SendSms(message, "7" + address, out error);
			if (smsId == 0) {
				Error($"не отправлено {address}, {error}");
			}
			var innerSmsMessage = new InnerSmsMessage() {
				UserTo = userTo,
				UserFrom = userFrom,
				TargetAddress = address,
				SmsId = smsId,
				Message = message,
				Date = SystemTime.Now()
			};
			DbSession.Save(innerSmsMessage);
			RedirectToAction("MessagesList", new {id});
		}

		public void Info(uint id)
		{
			PropertyBag["messageData"] = DbSession.Query<InnerSmsMessage>().FirstOrDefault(s=>s.Id == id);
		 }
	}
}