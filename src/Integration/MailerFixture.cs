using System;
using System.Linq;
using System.Net.Mail;
using AdminInterface.Controllers;
using AdminInterface.Models;
using AdminInterface.Models.Audit;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Suppliers;
using AdminInterface.MonoRailExtentions;
using Castle.ActiveRecord;
using Castle.Core.Smtp;
using Castle.MonoRail.Framework.Test;
using Castle.MonoRail.TestSupport;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using Integration.ForTesting;
using NUnit.Framework;
using Rhino.Mocks;
using Unit;

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
			message = null;
			controller = new RegisterController();

			PrepareController(controller, "Registered");
			((StubRequest)Request).Uri = new Uri("https://stat.analit.net/adm/Register/Register");
			((StubRequest)Request).ApplicationPath = "/Adm";

			ForTest.InitializeMailer();
			mailer = ForTest.TestMailer(m => message = m);

			payer = new Payer("Тестовый плательщик") {PayerID = 10};
			client = new Client(payer, Data.DefaultRegion) {
				Id = 58,
				Name = "Тестовый клиент",
				HomeRegion = new Region { Name = "test" },
				Settings = new DrugstoreSettings()
			};
			client.Users.Add(new User(client));
		}

		[Test]
		public void Enable_changed()
		{
			//просто сконструированный клиент больше не подходит 
			//тк в методе есть хак var client = Client.Find(((Service)item).Id); //(Client) item;
			//хибер конструирует прокси для item наследуя ее от Service тк он не знает кто это будет Client или Supplier
			//что бы обойти это и нужен хак, возможно есть какой то правильный сопсоб что бы 
			//хибер констуировал правельные прокми но я его не знаю
			//mailer.EnableChanged(client);

			using (new SessionScope())
			{
				var client = DataMother.TestClient(c => c.Name = "Тестовый клиент");
				mailer.EnableChanged(client);
				mailer.Send();
				Assert.That(message.Body, Is.StringContaining("Наименование: Тестовый клиент"));
			}
		}

		[Test]
		public void BillingNotificationTest()
		{
			payer.Comment = "";
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
Название: <a href=""https://stat.analit.net/adm/Payers/10"">Тестовый плательщик</a>
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
			payer.Comment = "";
			mailer.NotifyBillingAboutClientRegistration(client);
			mailer.Send();

			Assert.That(message.Body, Is.EqualTo(
				@"Зарегистрирован новый клиент
<br>
Название: <a href=""https://stat.analit.net/adm/Payers/10"">Тестовый плательщик</a>
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

				mailer.InvoiceToEmail(invoice, false);
				mailer.Send();
			}

			Assert.That(message.Body, Is.StringContaining("Примите счет за информационное обслуживание в ИС АналитФармация."));
		}

		[Test]
		public void Send_invoice_to_minimail_as_attachment()
		{
			using(new SessionScope())
			{
				var invoice = CreateInvoice();

				mailer.SendInvoiceToMinimail(invoice);
				mailer.Send();
			}
			Assert.That(message.Body, Is.StringContaining("Примите счет за информационное обслуживание в ИС АналитФармация."));
			Assert.That(message.Attachments.Count, Is.EqualTo(1));
			Assert.That(message.Attachments[0].Name, Is.EqualTo("Счет.html"));
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
			mailer.RevisionAct(act, "kvasovtest@analit.net", "");
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
			var oldClient = new Client(oldPayer, Data.DefaultRegion) {
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
			var oldClient = new Client(oldPayer, Data.DefaultRegion) {
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

		[Test]
		public void Change_Name_Full_Name()
		{
			var client = new Client(new Payer("Тестовы плательщик", "Тестовы плательщик"), Data.DefaultRegion) {
				Name = "Тестовый клиент",
				FullName = "Тестовый клиент"
			};
			var oldValue = client.Name;
			client.Name += "1";
			var property = new AuditableProperty(payer.GetType().GetProperty("Name"), "Наименование", client.Name, oldValue);

			mailer.NotifyAboutChanges(property, client);
			Assert.That(message.Subject, Is.EqualTo("Изменено поле 'Наименование'"));
			Assert.That(message.To.ToString(), Is.EqualTo("RegisterList@subscribe.analit.net"));
			Assert.That(message.Body, Is.StringContaining("Изменено 'Наименование' было 'Тестовый клиент' стало 'Тестовый клиент1'"));
			Assert.That(message.Body, Is.StringContaining(DateTime.Now.Date.ToShortDateString()));
		}

		[Test]
		public void Accounting_changed()
		{
			using (new SessionScope())
			{
				var client = DataMother.CreateTestClientWithUser();
				var user = client.Users[0];
				var payer = user.Payer;
				user.Accounting.Payment = 200;
				user.Save();

				mailer.AccountChanged(user.Accounting);
				mailer.Send();
				Assert.That(message.Subject, Is.EqualTo(String.Format("Изменение стоимости Тестовый плательщик - {0}, test - {1}, Аптека", payer.Id, client.Id)));
				Assert.That(message.Body, Is.StringContaining("было 800,00р. стало 200,00р."));
			}
		}

		[Test]
		public void Diff_notify()
		{
			var oldValue = payer.Comment;
			payer.Comment += "\r\nТестовый комментарий";
			var property = new DiffAuditableProperty(payer.GetType().GetProperty("Comment"), "Комментарий", payer.Comment, oldValue);
			mailer.NotifyPropertyDiff(property, payer).Send();

			Assert.That(message.Body, Is.StringContaining("Изменено 'Комментарий'"));
		}

		private Invoice CreateInvoice()
		{
			var payer = DataMother.CreatePayerForBillingDocumentTest();
			var invoice = new Invoice(payer, new Period(2011, Interval.January), new DateTime(2010, 12, 27));
			var group = invoice.Payer.ContactGroupOwner.AddContactGroup(ContactGroupType.Invoice);
			group.AddContact(ContactType.Email, "kvasovtest@analit.net");
			invoice.Save();
			return invoice;
		}
	}
}