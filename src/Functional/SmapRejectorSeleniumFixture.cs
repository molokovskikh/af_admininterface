using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.Models.Logs;
using Common.Web.Ui.Models.Audit;
using NUnit.Framework;
using Test.Support.Selenium;

namespace Functional
{
	[TestFixture]
	public class SmapRejectorSeleniumFixture : SeleniumFixture
	{
		private RejectedEmail spam;

		[SetUp]
		public void Setup()
		{
			spam = new RejectedEmail {
				Comment = "test",
				LogTime = DateTime.Now,
				MessageType = RejectedMessageType.MiniMail,
				SmtpId = 1000,
				Subject = "test subject",
				From = "test@test.com",
				To = "test_referrer@test.com"
			};
			session.Save(spam);
			defaultUrl = "SmapRejector/Show";
		}

		[Test]
		public void Referrer_and_type_test()
		{
			AssertText("Получатель");
			AssertText("test_referrer@test.com");
			AssertText("Тип сообщения");
			AssertText("Мини-почта");
		}
	}
}
