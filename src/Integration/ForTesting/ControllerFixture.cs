using System.Collections.Generic;
using System.Net.Mail;
using AdminInterface.MonoRailExtentions;
using Castle.ActiveRecord;
using Castle.Core.Smtp;
using Castle.MonoRail.TestSupport;
using NUnit.Framework;
using Rhino.Mocks;

namespace Integration.ForTesting
{
	public class ControllerFixture : BaseControllerTest
	{
		protected List<MailMessage> notifications;
		protected SessionScope session;

		[SetUp]
		public void Setup()
		{
			//Services.UrlBuilder.UseExtensions = false;

			notifications = new List<MailMessage>();

			var sender = MockRepository.GenerateStub<IEmailSender>();
			ForTest.InitializeMailer();
			sender.Stub(s => s.Send(new MailMessage())).IgnoreArguments()
				.Repeat.Any()
				.Callback(new Delegates.Function<bool, MailMessage>(m => {
					notifications.Add(m);
					return true;
				}));
			MailerExtention.SenderForTest = sender;

			session = new SessionScope();
		}

		[TearDown]
		public void TearDown()
		{
			if (session != null)
				session.Dispose();
		}
	}
}