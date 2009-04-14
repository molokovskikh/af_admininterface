using System;
using System.Net.Mail;
using System.Text;
using log4net;
using LumiSoft.Net.SMTP.Client;

namespace AdminInterface.Helpers
{
	public class Func
	{
		private static readonly ILog _log = LogManager.GetLogger(typeof (Func));

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
			try
			{
				if (!String.IsNullOrEmpty(to))
				{
#if DEBUG
					to = "r.kvasov@analit.net";
					bcc = "";
#endif
					var message = new MailMessage
					              	{
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

					var client = new SmtpClient("mail.adc.analit.net");
					client.Send(message);
				}
			}
			catch (Exception ex)
			{
				_log.Error(Utils.ExceptionToString(ex));
			}
		}

		public static void SendWitnStandartSender(MailMessage message)
		{
			try
			{
#if DEBUG
				message.To.Clear();
				message.CC.Clear();
				message.Bcc.Clear();

				message.To.Add("r.kvasov@analit.net");
#endif

				var client = new SmtpClient("mail.adc.analit.net");
				client.Send(message);
			}
			catch (Exception ex)
			{
				_log.Error(Utils.ExceptionToString(ex));
			}
		}

		public static string GeneratePassword()
		{
			var availableChars = "23456789qwertyupasdfghjkzxcvbnmQWERTYUPASDFGHJKLZXCVBNM";
			var password = String.Empty;
			var random = new Random();
			while (password.Length < 8)
				password += availableChars[random.Next(0, availableChars.Length - 1)];
			return password;
		}

		public static int Send(MailMessage message)
		{
			try
			{
#if DEBUG
				message.To.Clear();
				message.To.Add("r.kvasov@analit.net");
				message.Bcc.Clear();
				message.CC.Clear();
#endif
				var smtpid = SmtpClientEx.QuickSendSmartHostSMTPID("mail.adc.analit.net", "", "", message);
				if (smtpid == null)
					return 0;

				return smtpid.Value;
			}
			catch (Exception ex)
			{
				_log.Error(Utils.ExceptionToString(ex));
			}
			return 0;
		}
	}
}