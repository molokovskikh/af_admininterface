using System.Collections.Generic;
using System.Net.Mail;
using AdminInterface.MonoRailExtentions;
using Castle.ActiveRecord;
using Castle.Core.Smtp;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Routing;
using Castle.MonoRail.Framework.Services;
using Castle.MonoRail.Framework.Test;
using Castle.MonoRail.TestSupport;
using NUnit.Framework;
using Rhino.Mocks;

namespace Integration.ForTesting
{
	public class ControllerFixture : BaseControllerTest
	{
		protected List<MailMessage> notifications;
		protected ISessionScope session;
		protected string referer;

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

			session = new TransactionlessSession();
		}

		[TearDown]
		public void TearDown()
		{
			if (session != null)
				session.Dispose();
		}

		protected override IMockResponse BuildResponse(UrlInfo info)
		{
			return new StubResponse(info,
				new DefaultUrlBuilder(),
				new StubServerUtility(),
				new RouteMatch(),
				referer);
		}
	}
}