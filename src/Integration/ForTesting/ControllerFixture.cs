using System.Collections.Generic;
using System.Net.Mail;
using AdminInterface.MonoRailExtentions;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.Components.Validator;
using Castle.Core.Smtp;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Routing;
using Castle.MonoRail.Framework.Services;
using Castle.MonoRail.Framework.Test;
using Castle.MonoRail.TestSupport;
using Common.Web.Ui.Controllers;
using Common.Web.Ui.MonoRailExtentions;
using NHibernate;
using NUnit.Framework;
using Rhino.Mocks;
using Test.Support;

namespace Integration.ForTesting
{
	public class ControllerFixture : BaseControllerTest
	{
		protected List<MailMessage> notifications;
		protected ISessionScope scope;
		protected string referer;
		protected ValidatorRunner validator;
		protected ISession session;

		[SetUp]
		public void Setup()
		{
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

			Open();
		}

		[TearDown]
		public void TearDown()
		{
			Close();
		}

		protected void Reopen()
		{
			Close();
			Open();
		}

		private void Open()
		{
			scope = new TransactionlessSession();
			var sessionHolder = ActiveRecordMediator.GetSessionFactoryHolder();
			session = sessionHolder.CreateSession(typeof(ActiveRecordBase));
		}

		private void Close()
		{
			if (session != null) {
				session.Flush();
				ActiveRecordMediator.GetSessionFactoryHolder().ReleaseSession(session);
				session = null;
			}
			if (scope != null) {
				scope.Dispose();
				scope = null;
			}
		}

		public void Prepare(SmartDispatcherController controller)
		{
			controller.Validator = validator;
			controller.Binder.Validator = validator;
			var appController = controller as BaseController;
			if (appController != null)
				appController.DbSession = session;

			PrepareController(controller);
		}

		public void Flush()
		{
			session.Flush();
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