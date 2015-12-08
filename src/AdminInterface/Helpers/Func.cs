using System;
using System.Configuration;
using System.Net.Mail;
using System.Text;
using log4net;
using LumiSoft.Net.SMTP.Client;
using System.Net;
using System.IO;
using System.Web;
using System.Collections.Generic;

namespace AdminInterface.Helpers
{
	public class Func
	{
		private static readonly ILog _log = LogManager.GetLogger(typeof(Func));

		public static void Mail(string from,
			string subject,
			string body,
			string to)
		{
			Mail(from, String.Empty, subject, body, to, String.Empty, String.Empty);
		}

		public static void Mail(string from,
			string subject,
			string body,
			string to,
			string bcc)
		{
			Mail(from, String.Empty, subject, body, to, String.Empty, bcc);
		}


		public static void Mail(string from,
			string fromDisplayName,
			string subject,
			string body,
			string to,
			string toDisplayName,
			string bcc)
		{
			try {
				if (!String.IsNullOrEmpty(to)) {
#if DEBUG
					to = GetDebugMail();
					bcc = "";
#endif
					var message = new MailMessage {
						From = new MailAddress(from, fromDisplayName, Encoding.UTF8),
						IsBodyHtml = false,
						Subject = subject,
						SubjectEncoding = Encoding.UTF8,
						Body = body,
						BodyEncoding = Encoding.UTF8,
					};
					if (!String.IsNullOrEmpty(bcc))
						message.Bcc.Add(bcc);

					foreach (var toAddress in to.Split(",".ToCharArray()))
						message.To.Add(new MailAddress(toAddress, toDisplayName, Encoding.UTF8));

					var client = new SmtpClient(GetSmtpServer());
					client.Send(message);
				}
			}
			catch (Exception ex) {
				_log.Error("Не удалось отправить письмо", ex);
			}
		}

		public static string GetSmtpServer()
		{
			return ConfigurationManager.AppSettings["SmtpServer"];
		}

		private static string GetDebugMail()
		{
			return ConfigurationManager.AppSettings["DebugMail"];
		}

		private static string GetSmsUri()
		{
			return ConfigurationManager.AppSettings["SmsUri"];
		}

		public static string GetSmsStatUri()
		{
			return ConfigurationManager.AppSettings["SmsStatUri"];
		}

		public static void SendWitnStandartSender(MailMessage message)
		{
			try {
#if DEBUG
				message.To.Clear();
				message.CC.Clear();
				message.Bcc.Clear();

				message.To.Add(GetDebugMail());
#endif

				var client = new SmtpClient(GetSmtpServer());
				client.Send(message);
			}
			catch (Exception ex) {
				_log.Error("Не удалось отправить письмо", ex);
			}
		}

		public static int Send(MailMessage message)
		{
			try {
#if DEBUG
				message.To.Clear();
				message.To.Add(GetDebugMail());
				message.Bcc.Clear();
				message.CC.Clear();
#endif
				var smtpid = SmtpClientEx.QuickSendSmartHostSMTPID(GetSmtpServer(), "", "", message);
				if (smtpid == null)
					return 0;

				return smtpid.Value;
			}
			catch (Exception ex) {
				_log.Error("Не удалось отправить письмо", ex);
			}
			return 0;
		}

		public static int SendSms(string message, string phone, out string error)
		{
			int result = 0;
			error = "";

			string smsUri = GetSmsUri();
			if (String.IsNullOrEmpty(smsUri)) {
				error = "не найден адрес сервиса отправки смс";
				_log.Error("Не удалось отправить SMS", new NotSupportedException(error));
				return result;
			}

			var r = (HttpWebRequest)(WebRequest.Create(smsUri));
			string data =$"text={HttpUtility.UrlEncode(message)}&dest={HttpUtility.UrlEncode(phone)}";

#if DEBUG
			error = "режим дебага, отправка сообщений отключена";
			return result;
#endif

			r.Method = "POST";
			var encoding = Encoding.UTF8;
			r.ContentLength = encoding.GetByteCount(data);
			r.Credentials = CredentialCache.DefaultCredentials;
			r.ContentType = "application/x-www-form-urlencoded";

			try {
				using (var requestStream = r.GetRequestStream())
					requestStream.Write(encoding.GetBytes(data), 0, encoding.GetByteCount(data));

				string responseBody = "";
				var response = (HttpWebResponse)(r.GetResponse());

				// если непредусмотренный ответ
				if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.BadRequest) {
					error = $"сервиc отправки смс ответил с http-статусом {response.StatusCode}";
					throw new NotSupportedException(error);
				}
				using (var rspStm = response.GetResponseStream())
				using (var reader = new StreamReader(rspStm, encoding))
					responseBody = reader.ReadToEnd();

				// если ответ BadRequest
				if (response.StatusCode == HttpStatusCode.BadRequest) {
					error = $"сервис отправки смс ответил {response.StatusCode}, \"{responseBody}\"";
					throw new NotSupportedException(error);
				}

				// ожидается ответ вида OK;3517
				var values = responseBody.Split(';');
				if (values.Length < 2 || values[0] != "OK" || String.IsNullOrEmpty(values[1]) || !Int32.TryParse(values[1], out result)) {
					error = $"ответ сервиса отправки смс имеет неправильный формат \"{responseBody}\"";
					throw new NotSupportedException(error);
				}

			}
			catch (Exception ex) {
				_log.Error("Не удалось отправить SMS", ex);
				return result;
			}

			return result;
		}
	}
}