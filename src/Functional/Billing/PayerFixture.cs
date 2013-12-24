using System;
using System.Threading;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Castle.ActiveRecord;
using Functional.ForTesting;
using Integration.ForTesting;
using NUnit.Framework;
using WatiN.Core;
using Test.Support.Web;
using WatiN.Core.Native.Windows;

namespace Functional.Billing
{
	[TestFixture]
	public class PayerFixture : WatinFixture2
	{
		[Test]
		public void Show_balance_summary()
		{
			var payer = DataMother.CreatePayerForBillingDocumentTest();
			var invoice = new Invoice(payer, new Period(2011, Interval.January), new DateTime(2011, 1, 11));
			session.Save(invoice);
			var payment = new Payment(payer, new DateTime(2011, 1, 15), 800);
			session.Save(payment);

			Open(payer);
			AssertText("плательщик");
			Click(String.Format(@"Платежи/Счета {0}", invoice.Period.Year));
			Thread.Sleep(1000);
			AssertText("11.01.2011");
			AssertText("15.01.2011");
		}

		[Test]
		public void Show_error_on_revision_act_in_not_configured_payer()
		{
			var payer = DataMother.CreatePayer();
			session.Save(payer);

			Open(payer);
			AssertText("Плательщик");
			Click("Акт сверки");
			AssertText("У плательщика не указан получатель платежей, выберете получателя платежей");
		}

		[Test]
		public void Create_act()
		{
			var payer = DataMother.CreatePayerForBillingDocumentTest();
			Open(payer);
			Click("Доп. акт");
			AssertText("Формирование дополнительного акта");
			Css("#act_Date").Value = DateTime.Now.ToShortDateString();
			Css("#act_parts_0__name").Value = "Информационные услуги за октябрь";
			Css("#act_parts_0__cost").Value = "800";
			Css("#act_parts_0__count").Value = "2";
			Click("Сохранить");
			AssertText("Акт сформирован");
		}

		[Test, Ignore("Не реализованно")]
		public void Custom_invoice()
		{
			var payer = DataMother.CreatePayerForBillingDocumentTest();
			Open(payer);
			Click("Новый счет");
			Css("#invoice_date").Value = DateTime.Now;
			Css("#invoice_bills[0]_Name").Value = "Информационные услуги за октябрь";
			Css("#invoice_bills[0]_Cost").Value = "500";
			Css("#invoice_bills[0]_Count").Value = "1";
			Click("Сохранить");
		}

		[Test]
		public void Delete_payer_dialog()
		{
			var payer = DataMother.CreatePayerForBillingDocumentTest();
			Open(payer);
			Click("Удалить Плательщика");
			AssertText("Введите причину удаления");
			Click("Продолжить");
			AssertText("Это поле необходимо заполнить.");
			Css("#CommentField").AppendText("123456");
			Click("Продолжить");
		}

		[Test]
		public void NotSendEmptyMessageTest()
		{
			var payer = DataMother.CreatePayerForBillingDocumentTest();
			Open(payer);
			ClickButton("Принять");
			AssertText("Это поле необходимо заполнить.");
			browser.TextField(Find.ByName("messageText")).TypeText("Тестовое сообщение");
			ClickButton("Принять");
			AssertText("Тестовое сообщение");
		}
	}
}