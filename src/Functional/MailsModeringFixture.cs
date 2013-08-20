using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using AdminInterface.Models;
using AdminInterface.Models.Documents;
using AdminInterface.Models.Suppliers;
using Integration.ForTesting;
using NUnit.Framework;
using Test.Support.Web;
using WatiN.Core;
using WatiN.Core.Native.Windows;

namespace Functional
{
	[TestFixture]
	public class MailsModeringFixture : WatinFixture2
	{
		protected Supplier Supplier;
		protected Client Client;
		protected Mail Mail;

		[SetUp]
		public void SetUp()
		{
			session.CreateSQLQuery("delete from documents.Mails;").ExecuteUpdate();

			Supplier = DataMother.CreateSupplier();
			session.Save(Supplier);
			Client = DataMother.CreateTestClientWithAddressAndUser();
			session.Save(Client);

			Mail = new Mail {
				LogTime = DateTime.Now,
				Supplier = Supplier,
				Subject = "testSubject",
				Body = "test body mail supplier",
				SupplierEmail = "test@mail.ru"
			};
			Mail.Recipients.Add(new MailRecipient { Region = Supplier.HomeRegion, Mail = Mail, Type = RecipientType.Region });
			Mail.Recipients.Add(new MailRecipient { Client = Client, Mail = Mail, Type = RecipientType.Client });
			Mail.Recipients.Add(new MailRecipient { Address = Client.Addresses[0], Mail = Mail, Type = RecipientType.Address });

			session.Save(Mail);

			session.Save(new Attachment { Filename = "testFileName", Extension = ".ttt", Mail = Mail });

			var log1 = new MailSendLog { Committed = true, Mail = Mail, User = Client.Users[0] };
			var log2 = new MailSendLog { Mail = Mail, User = Client.Users[0] };
			Save(log1, log2);
		}

		[Test]
		public void Base_show_Test()
		{
			Open();
			Click("Модерирование минипочты");
			AssertText("testSubject");
			AssertText(Supplier.Name);
			AssertText(Client.Name);
			AssertText(Client.Addresses[0].Name);
			AssertText("2 (1/1)");
		}

		[Test]
		public void Show_mail_test()
		{
			Open();
			Click("Модерирование минипочты");
			AssertText("testSubject");
			browser.Link(Find.ByClass("ShowMailLink")).Click();
			Thread.Sleep(500);
			AssertText("test body mail supplier");
			AssertText("testFileName");
		}

		[Test]
		public void Delete_mail_test()
		{
			Open();
			Click("Модерирование минипочты");
			Click("Удалить");
			WaitForText("Удалить письмо");
			Click("Да");
			Thread.Sleep(2000);
			Close();
			Assert.IsTrue(session.Get<Mail>(Mail.Id).Deleted);
		}
	}
}
