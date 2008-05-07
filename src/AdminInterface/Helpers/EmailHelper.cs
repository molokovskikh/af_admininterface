using System;
using System.Net.Mail;

namespace AdminInterface.Helpers
{
	public class EmailHelper
	{
		public static void BuildAttachementFromString(string to, MailMessage message)
		{
			if (String.IsNullOrEmpty(to))
				return;

			foreach (var email in to.Split(','))
			{
				var normilizedEmail = email.Trim('\r', '\n', '\t', ' ');
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
	}
}
