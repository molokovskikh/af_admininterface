using System.Net.Mail;
using AdminInterface.Helpers;
using NUnit.Framework;

namespace Unit
{
	[TestFixture]
	public class EmailHelperFixture
	{
		[Test]
		public void BuildAttachementFromStringTest()
		{
			var message = new MailMessage();
			EmailHelper.BuildAttachementFromString(@"
r.kvasov@analit.net

", message);
			Assert.That(message.To.Count, Is.EqualTo(1), "количество записей в поле to меньше одной");
		}

		[Test]
		public void NormalizeEmailOrPhoneTest()
		{
			Assert.That(EmailHelper.NormalizeEmailOrPhone(null), Is.Null);
			Assert.That(EmailHelper.NormalizeEmailOrPhone(""), Is.Empty);
			Assert.That(EmailHelper.NormalizeEmailOrPhone(" \r \r  \n 456-456546   \t \r\n "), Is.EqualTo("456-456546"));
			Assert.That(EmailHelper.NormalizeEmailOrPhone("456-456546"), Is.EqualTo("456-456546"));
		}

		[Test]
		public void JoinMails()
		{
			Assert.That(EmailHelper.JoinMails(null, "tech@analit.net"), Is.EqualTo("tech@analit.net"));
			Assert.That(EmailHelper.JoinMails("tech@analit.net", ""), Is.EqualTo("tech@analit.net"));
			Assert.That(EmailHelper.JoinMails("tech@analit.net", "service@analit.net"), Is.EqualTo("tech@analit.net, service@analit.net"));
		}
	}
}