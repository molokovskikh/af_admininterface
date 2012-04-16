using System.Collections.Generic;
using System.Net.Mail;
using AdminInterface.MonoRailExtentions;
using Castle.ActiveRecord;
using Castle.Components.Validator;
using Castle.Core.Smtp;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Routing;
using Castle.MonoRail.Framework.Services;
using Castle.MonoRail.Framework.Test;
using Castle.MonoRail.TestSupport;
using Common.Web.Ui.MonoRailExtentions;
using NUnit.Framework;
using Rhino.Mocks;

namespace Integration.ForTesting
{
	public class ControllerFixture : BaseControllerTest
	{
		protected List<MailMessage> notifications;
		protected ISessionScope scope;
		protected string referer;
		protected ValidatorRunner validator;

		[SetUp]
		public void Setup()
		{
			//Services.UrlBuilder.UseExtensions = false;

			validator = new ValidatorRunner(new CachedValidationRegistry());

			notifications = new List<MailMessage>();

			var sender = MockRepository.GenerateStub<IEmailSender>();
			ForTest.InitializeMailer();
			sender.Stub(s => s.Send(new MailMessage())).IgnoreArguments()
				.Repeat.Any()
				.Callback(new Delegates.Function<bool, MailMessage>(m => {
					notifications.Add(m);
					return true;
				}));
			BaseMailerExtention.SenderForTest = sender;

			scope = new TransactionlessSession();
		}

		public void Prepare(SmartDispatcherController controller)
		{
			controller.Validator = validator;
			controller.Binder.Validator = validator;
			PrepareController(controller);
		}

		[TearDown]
		public void TearDown()
		{
			if (scope != null)
				scope.Dispose();
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