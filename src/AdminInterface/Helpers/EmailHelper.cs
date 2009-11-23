using System;
using System.Linq;
using System.Net.Mail;
using Common.Tools;

namespace Common.Web.Ui.Helpers
{
	public class EmailHelper
	{
		public static void BuildAttachementFromString(string to, MailMessage message)
		{
			if (String.IsNullOrEmpty(to))
				return;

			foreach (var email in to.Split(','))
			{
				var normilizedEmail = NormalizeEmailOrPhone(email);
				if (normilizedEmail.Length == 0)
					continue;

				if (!IsEmailExists(message, normilizedEmail))
					message.To.Add(normilizedEmail);
			}
		}

		public static bool IsEmailExists(MailMessage message, string email)
		{
			foreach (var address in message.To)
				if (String.Compare(address.Address, email, StringComparison.InvariantCultureIgnoreCase) == 0)
					return true;

			return false;
		}

		public static string NormalizeEmailOrPhone(string text)
		{
			if (text == null)
				return null;
			return text.Trim();
		}

		public static string JoinMails(params string[] parts)
		{
			return parts.Where(p => !String.IsNullOrEmpty(p) && !String.IsNullOrEmpty(p.Trim())).Implode(", ");
		}
	}
}