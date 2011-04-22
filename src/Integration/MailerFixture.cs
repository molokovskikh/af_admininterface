using System;
using System.Linq;
using System.Net.Mail;
using AdminInterface.Controllers;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.MonoRailExtentions;
using Castle.ActiveRecord;
using Castle.Core.Smtp;
using Castle.MonoRail.Framework.Test;
using Castle.MonoRail.TestSupport;
using Common.Web.Ui.Models;
using Integration.ForTesting;
using NUnit.Framework;
using Rhino.Mocks;

namespace Integration
{
	[TestFixture]
	public class MailerFixture : BaseControllerTest
	{
		private RegisterController controller;
		private MonorailMailer mailer;
		private MailMessage message;
		private Client client;
		private Payer payer;

		[SetUp]
		public void Setup()
		{
			controller = new RegisterController();

			PrepareController(controller, "Registered");
			((StubRequest)Request).Uri = new Uri("https://stat.analit.net/adm/Register/Register");
			((StubRequest)Request).ApplicationPath = "/Adm";
			var sender = MockRepository.GenerateStub<IEmailSender>();
			sender.Stub(s => s.Send(message)).IgnoreArguments()
				.Repeat.Any()
				.Callback(new Delegates.Function<bool, MailMessage>(m => {
					message = m;
					return true;
				}));
			mailer = new MonorailMailer(sender) {
				UnderTest = true,
				Controller = controller
			};

			ForTest.InitializeMailer();

			client = new Client {
				Id = 58,
				Name = "Тестовый клиент",
				HomeRegion = new Region { Name = "test" },
				Settings = new DrugstoreSettings()
			};
			payer = new Payer("Тестовый плательщик") { PayerID = 10};
			client.JoinPayer(payer);
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
			payer.AddComment(paymentOptions.GetCommentForPayer());

			mailer.NotifyBillingAboutClientRegistration(client);
			mailer.Send();

			Assert.That(message, Is.Not.Null, "Сообщение не послано");
			Assert.That(message.To.ToString(), Is.EqualTo("billing@analit.net"));

			Assert.That(message.From.ToString(), Is.EqualTo("register@analit.net"));

			Assert.That(message.Subject, Is.EqualTo("Регистрация нового клиента"));
			Assert.That(message.Body, Is.EqualTo(
				@"Зарегистрирован новый клиент
<br>
Название: <a href='https://stat.analit.net/Adm/Billing/edit.rails?BillingCode=10'>Тестовый плательщик</a>
<br>
Код: 58
<br>
Биллинг код: 10
<br>
Кем зарегистрирован: test
<br>
Клиент обслуживается бесплатно
"));
			Assert.That(message.IsBodyHtml, Is.True);
		}

		[Test]
		public void Billing_notification_for_client_with_basic_submission()
		{
			var paymentOptions = new PaymentOptions { PaymentPeriodBeginDate = new DateTime(2007, 1, 1), Comment = "Test comment"};
			payer.AddComment(paymentOptions.GetCommentForPayer());

			mailer.NotifyBillingAboutClientRegistration(client);
			mailer.Send();

			Assert.That(message.Body, Is.StringContaining(@"
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
Название: <a href='https://stat.analit.net/Adm/Billing/edit.rails?BillingCode=10'>Тестовый плательщик</a>
<br>
Код: 58
<br>
Биллинг код: 10
<br>
Кем зарегистрирован: test
<br>

"));
		}

		[Test]
		public void Send_invoice()
		{
			using(new SessionScope())
			{
				var invoice = CreateInvoice();

				mailer.Invoice(invoice);
				mailer.Send();
			}

			Assert.That(message.Body, Is.StringContaining("Примите счет за информационное обслуживание в ИС АналитФармация."));
		}

		[Test]
		public void Broken_invoice()
		{
			using (new SessionScope())
			{
				var invoice = CreateInvoice();

				mailer.DoNotHaveInvoiceContactGroup(invoice);
				mailer.Send();

				Assert.That(message.Body, Is.StringContaining(String.Format("Не удалось отправить счет № {0}", invoice.Id)));
			}
		}

		[Test]
		public void Revision_act()
		{
			var act = DataMother.GetAct();
			mailer.RevisionAct(act, "kvasovtest@analit.net");
			mailer.Send();
			Assert.That(message.Subject, Is.EqualTo("Акт сверки"));
			Assert.That(message.From.ToString(), Is.EqualTo("billing@analit.net"));
			Assert.That(message.To.ToString(), Is.EqualTo("kvasovtest@analit.net"));
			Assert.That(message.Body, Is.StringContaining("Во вложении акт сверки на 01.02.2011"));
			Assert.That(message.Attachments.Count, Is.EqualTo(1));
			Assert.That(message.Attachments[0].Name, Is.EqualTo("Акт сверки.xls"));
		}

		[Test]
		public void Send_user_move_notification()
		{
			var user = new User(client) {
				Id = 1,
				Login = "1"
			};
			client.Name = "Фармаимпекс";
			user.Payer.Name = "Фармаимпекс";
			var oldPayer = new Payer {
				Name = "Биона"
			};
			var oldClient = new Client(oldPayer) {
				Name = "Биона"
			};
			mailer.UserMoved(user, oldClient, oldPayer);
			mailer.Send();
			Assert.That(message.Subject, Is.EqualTo("Перемещение пользователя"));
			Assert.That(message.To.ToString(), Is.EqualTo("RegisterList@subscribe.analit.net"));
			Assert.That(message.Body, Is.EqualTo(@"Пользователь 1 перемещен
Старый клиент Биона плательщик Биона
Новый клиент Фармаимпекс плательщик Фармаимпекс"));
		}

		[Test]
		public void Send_address_move_notification()
		{
			var address = new Address(client, payer.JuridicalOrganizations.First()) {
				Value = "N14 г.Бугульма, ул.Якупова, д.40"
			};
			client.Name = "Фармаимпекс";
			address.Payer.Name = "Фармаимпекс";
			address.LegalEntity.Name = "Фармаимпекс";

			var oldPayer = new Payer("Биона");
			var oldLegalEntity = oldPayer.JuridicalOrganizations.First();
			var oldClient = new Client(oldPayer) {
				Name = "Биона"
			};
			mailer.AddressMoved(address, oldClient, oldLegalEntity);
			mailer.Send();
			Assert.That(message.Subject, Is.EqualTo("Перемещение адреса доставки"));
			Assert.That(message.To.ToString(), Is.EqualTo("RegisterList@subscribe.analit.net"));
			Assert.That(message.Body, Is.EqualTo(@"Адрес N14 г.Бугульма, ул.Якупова, д.40
Старый клиент Биона плательщик Биона юр.лицо Биона
Новый клиент Фармаимпекс плательщик Фармаимпекс юр.лицо Фармаимпекс"));
		}

		private Invoice CreateInvoice()
		{
			var payer = DataMother.BuildPayerForBillingDocumentTest();
			var invoice = new Invoice(payer, Period.January, new DateTime(2010, 12, 27));
			var group = invoice.Payer.ContactGroupOwner.AddContactGroup(ContactGroupType.Invoice);
			group.AddContact(new Contact(ContactType.Email, "kvasovtest@analit.net"));
			invoice.Save();
			return invoice;
		}
	}
}