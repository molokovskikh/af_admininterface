using System;
using System.Net.Mail;
using AddUser;
using AdminInterface.Models;

namespace AdminInterface.Services
{
	public class NotificationService
	{
		private readonly Action<MailMessage> _sendMessage;

		public NotificationService(Action<MailMessage> sendMessage)
		{
			_sendMessage = sendMessage;
		}

		public NotificationService()
		{
			_sendMessage = Func.SendWitnStandartSender;
		}

		public void SendNotificationToBillingAboutClientRegistration(uint clientCode,
		                                                             uint payerId,
		                                                             string clientName,
		                                                             string userName,
		                                                             bool isBasicSubmission,
		                                                             PaymentOptions options)
		{
			var message = new MailMessage();
			message.To.Add("billing@analit.net");
			message.From = new MailAddress("register@analit.net");
			message.IsBodyHtml = true;
			message.Subject = "Регистрация нового клиента";

			string clientType;

			if (isBasicSubmission)
				clientType = "Клиент с Базовым подчинением";
			else
				clientType = "Независимая копия";

			var paymentOptions = "";
			if (options != null)
				paymentOptions = "<br>" + options.GetCommentForPayer().Replace("\r\n", "<br>");

			message.Body = String.Format(
@"Зарегистрирован новый клиент
<br>
Название: <a href='https://stat.analit.net/Adm/Billing/edit.rails?clientCode={1}'>{0}</a>
<br>
Код: {1}
<br>
Биллинг код: {2}
<br>
Кем зарегистрирован: {3}
<br>
{4}
{5}", clientName, clientCode, payerId, userName, clientType, paymentOptions).Replace(Environment.NewLine, "");

			_sendMessage(message);
		}
	}
}
