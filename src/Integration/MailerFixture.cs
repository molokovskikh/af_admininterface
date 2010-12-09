using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using AdminInterface.Controllers;
using AdminInterface.Models;
using AdminInterface.Models.Security;
using AdminInterface.MonoRailExtentions;
using AdminInterface.Security;
using Castle.Components.Common.EmailSender;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Configuration;
using Castle.MonoRail.Framework.Internal;
using Castle.MonoRail.Framework.Services;
using Castle.MonoRail.Framework.Test;
using Castle.MonoRail.TestSupport;
using Castle.MonoRail.Views.Brail;
using IgorO.ExposedObjectProject;
using NUnit.Framework;
using Rhino.Mocks;
using Message = Castle.Components.Common.EmailSender.Message;

namespace Integration
{
	public class TestServiceProvider : IServiceProvider
	{
		public Dictionary<Type, object> Services = new Dictionary<Type, object>();

		public object GetService(Type serviceType)
		{
			Console.WriteLine(serviceType);
			if (!Services.ContainsKey(serviceType))
				return null;
			return Services[serviceType];
		}
	}

	[TestFixture]
	public class MailerFixture : BaseControllerTest
	{
		private RegisterController controller;
		private MonorailMailer mailer;
		private Message message;
		private Client client;

		[SetUp]
		public void Setup()
		{
			SecurityContext.GetAdministrator = () => new Administrator {UserName = "TestAdmin", ManagerName = "test"};
			controller = new RegisterController();

			var sender = MockRepository.GenerateStub<IEmailSender>();
			sender.Stub(s => s.Send(message)).IgnoreArguments()
				.Repeat.Any()
				.Callback(new Delegates.Function<bool, Message>(m => {
					message = m;
					return true;
				}));
			mailer = new MonorailMailer(sender);
			mailer.UnderTest = true;
			PrepareController(controller, "Registered");
			((StubRequest)Request).Uri = new Uri("https://stat.analit.net/adm/Register/Register");
			((StubRequest)Request).ApplicationPath = "/Adm";
			mailer.Controller = controller;
			controller.Context.Services.EmailTemplateService = new EmailTemplateService(GetViewManager());

			client = new Client
			{
				Id = 58,
				Payer = new Payer { PayerID = 10 },
				Name = "Тестовый клиент",
				HomeRegion = new Region { Name = "test" }
			};
		}

		public static IViewEngineManager GetViewManager()
		{
			var config = new MonoRailConfiguration();
			config.ViewEngineConfig.ViewEngines.Add(new ViewEngineInfo(typeof(BooViewEngine), false));
			config.ViewEngineConfig.ViewPathRoot = Path.Combine(@"..\..\..\AdminInterface", "Views");

			var provider = new TestServiceProvider();
			provider.Services.Add(typeof(IMonoRailConfiguration), config);
			provider.Services.Add(typeof(IViewSourceLoader), new FileAssemblyViewSourceLoader(config.ViewEngineConfig.ViewPathRoot));

			var manager = new DefaultViewEngineManager();
			manager.Service(provider);
			manager.Initialize();
			var namespaces = ExposedObject.From(manager).viewEnginesFastLookup[0].Options.NamespacesToImport;
			namespaces.Add("Boo.Lang.Builtins");
			namespaces.Add("AdminInterface.Helpers");
			return manager;
		}

		[Test]
		public void Enable_changed()
		{
			mailer.EnableChanged(client, false);
			mailer.Send();
			Assert.That(message.Body, Is.StringContaining("Наименование клиента: Тестовый клиент"));
		}

		[Test]
		public void BillingNotificationTest()
		{
			var paymentOptions = new PaymentOptions{ WorkForFree = true, Comment = "Независимая копия" };
			client.Payer.AddComment(paymentOptions.GetCommentForPayer());

			mailer.NotifyBillingAboutClientRegistration(client);
			mailer.Send();

			Assert.That(message, Is.Not.Null, "Сообщение не послано");
			Assert.That(message.To, Is.EqualTo("billing@analit.net"));

			Assert.That(message.From, Is.EqualTo("register@analit.net"));

			Assert.That(message.Subject, Is.EqualTo("Регистрация нового клиента"));
			Assert.That(message.Body, Is.EqualTo(
				@"Зарегистрирован новый клиент
<br>
Название: <a href='https://stat.analit.net/Adm/Billing/edit.rails?clientCode=58'>Тестовый клиент</a>
<br>
Код: 58
<br>
Биллинг код: 10
<br>
Кем зарегистрирован: test
<br>
Клиент обслуживается бесплатно
"));
			Assert.That(message.Format, Is.EqualTo(Format.Html));
		}

		[Test]
		public void Billing_notification_for_client_with_basic_submission()
		{
			var paymentOptions = new PaymentOptions { PaymentPeriodBeginDate = new DateTime(2007, 1, 1), Comment = "Test comment"};
			client.Payer.AddComment(paymentOptions.GetCommentForPayer());

			mailer.NotifyBillingAboutClientRegistration(client);
			mailer.Send();

			Assert.That(message.Body, Is.EqualTo(
				@"Зарегистрирован новый клиент
<br>
Название: <a href='https://stat.analit.net/Adm/Billing/edit.rails?clientCode=58'>Тестовый клиент</a>
<br>
Код: 58
<br>
Биллинг код: 10
<br>
Кем зарегистрирован: test
<br>
Дата начала платного периода: 01.01.2007
Комментарий: Test comment
"));
		}

		[Test]
		public void Billing_notification_for_client_without_payment_options()
		{
			mailer.NotifyBillingAboutClientRegistration(client);
			mailer.Send();

			Assert.That(message.Body, Is.EqualTo(
				@"Зарегистрирован новый клиент
<br>
Название: <a href='https://stat.analit.net/Adm/Billing/edit.rails?clientCode=58'>Тестовый клиент</a>
<br>
Код: 58
<br>
Биллинг код: 10
<br>
Кем зарегистрирован: test
<br>

"));
		}
	}
}