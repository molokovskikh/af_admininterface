using AdminInterface.Models.Logs;
using NUnit.Framework;

namespace Unit.Models
{
	[TestFixture]
	public class PasswordChangeLogEntityFixture
	{
		[Test]
		public void Sent_to_must_be_set_into_concatenatio_of_client_mails_and_adddition_mails()
		{
			var passwordChanged = new PasswordChangeLogEntity();
			passwordChanged.SetSentTo(1, "r.kvasov@analit.net");
			Assert.That(passwordChanged.SmtpId, Is.EqualTo(1));
			Assert.That(passwordChanged.SentTo, Is.EqualTo("r.kvasov@analit.net"));
		}
	}
}
