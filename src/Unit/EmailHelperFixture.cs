using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using AdminInterface.Helpers;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace AdminInterface.Test.Helpers
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

",message);
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
	}
}
