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
	}
}
