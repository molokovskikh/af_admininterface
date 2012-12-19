using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using AdminInterface.Controllers;
using AdminInterface.Models.Billing;
using Common.Tools;
using Integration.ForTesting;
using NUnit.Framework;

namespace Integration.Models
{
	[TestFixture]
	public class ActFixture : Test.Support.IntegrationFixture
	{
		[Test]
		public void Do_not_build_duplicate_document()
		{
			var payer = DataMother.CreatePayerForBillingDocumentTest();
			var invoice = new Invoice(payer, new Period(2010, Interval.December), DateTime.Now);
			invoice.Save();

			var acts = Act.Build(new List<Invoice> { invoice }, DateTime.Now);
			acts.Each(a => a.Save());

			Assert.That(acts.Count(), Is.EqualTo(1));

			acts = Act.Build(new List<Invoice> { invoice }, DateTime.Now);
			Assert.That(acts.Count(), Is.EqualTo(0));
		}

		[Test]
		public void NotifyAboutDeleteAct()
		{
			var act = PrepareAct();
			MailMessage message = null;
			var mailer = ForTest.TestMailer(m => message = m);
			mailer.DeleteOrEditAct(act, "testmail@test.te", "Удален акт", true).Send();
			Assert.That(message, Is.Not.Null);
			Assert.That(message.Body, Is.StringStarting(String.Format(@"{0}: <br/>
Документ №{1}, дата: {2}<br/>
Период: {5}<br/>
Сумма: {6}<br/>
Организация: {3}<br/>
Контрагент от Аналит: {4}<br/>
Пользователь: test<br/>
Дата и время удаления:", "Удален акт", act.Id, act.ActDate.ToString("dd.MM.yyyy"),
				act.Customer, act.Recipient.Name, new Period(2012, Interval.January), 100)));
		}
		private Act PrepareAct()
		{
			ForTest.InitializeMailer();
			return new Act {
				ActDate = new DateTime(2012, 11, 13),
				Recipient = new Recipient {
					Name = "Тестовый получатель"
				},
				Customer = "Организация",
				Period = new Period(2012, Interval.January),
				Sum = 100
			};
		}
		[Test]
		public void NotifyAboutEditAct()
		{
			var act = PrepareAct();
			MailMessage message = null;
			var mailer = ForTest.TestMailer(m => message = m);
			mailer.DeleteOrEditAct(act, "testmail@test.te", "Изменен акт", false).Send();
			Assert.That(message, Is.Not.Null);
			Assert.That(message.Body, Is.StringStarting(String.Format(@"{0}: <br/>
Документ №{1}, дата: {2}<br/>
Период: {5}<br/>
Сумма: {6}<br/>
Организация: {3}<br/>
Контрагент от Аналит: {4}<br/>
Пользователь: test<br/>
Дата и время изменения:", "Изменен акт", act.Id, act.ActDate.ToString("dd.MM.yyyy"),
				act.Customer, act.Recipient.Name, new Period(2012, Interval.January), 100)));
		}
	}
}