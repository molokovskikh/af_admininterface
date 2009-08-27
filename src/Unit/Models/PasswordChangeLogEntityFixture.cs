using AdminInterface.Models.Logs;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace AdminInterface.Test.Models
{
	[TestFixture]
	public class PasswordChangeLogEntityFixture
	{
		[Test]
		public void Sent_to_must_be_set_into_concatenatio_of_client_mails_and_adddition_mails()
		{
			var passwordChanged = new PasswordChangeLogEntity();
			passwordChanged.SetSentTo("r.kvasov@analit.net");
			Assert.That(passwordChanged.SentTo, Is.EqualTo("r.kvasov@analit.net"));
		}
	}
}
