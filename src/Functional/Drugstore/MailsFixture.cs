﻿using System;
using System.IO;
using System.Linq;
using AddUser;
using AdminInterface.Models;
using AdminInterface.Models.Documents;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Suppliers;
using Functional.ForTesting;
using Integration.ForTesting;
using NUnit.Framework;
using WatiN.Core;

namespace Functional.Drugstore
{
	public class MailsFixture : WatinFixture2
	{
		Client client;
		User user;
		Supplier supplier;

		Mail mail;
		MailSendLog log;

		AppConfig config;

		[SetUp]
		public void Setup()
		{
			client = DataMother.CreateTestClientWithAddressAndUser();
			user = client.Users[0];
			supplier = DataMother.CreateSupplier();
			supplier.Save();

			BuildMail();

			config = new AppConfig {
				AttachmentsPath = "../../../AdminInterface/Data/Attachments/"
			};
			if (!Directory.Exists("../../../AdminInterface/Data"))
				Directory.CreateDirectory("../../../AdminInterface/Data");

			if (!Directory.Exists(config.AttachmentsPath))
				Directory.CreateDirectory(config.AttachmentsPath);
		}

		private void BuildMail()
		{
			mail = new Mail {
				Supplier = supplier,
				Subject = "Тестовое сообщение",
				Body = "Привет,\r\nЭто тестовое сообщение",
				LogTime = DateTime.Now,
				SupplierEmail = "test@analit.net"
			};
			mail.AddRecipient(client);

			log = new MailSendLog {User = user, Mail = mail, Recipient = mail.Recipients[0]};

			session.Save(mail);
			session.Save(log);
		}

		[Test]
		public void Show_mails_history()
		{
			Open(client);
			AssertText("История минипочты");
			Click("История минипочты");
			AssertText("История сообщений минипочты");
			AssertText("Тестовое сообщение");
		}

		[Test]
		public void Show_attachments()
		{
			var attachment = new Attachment {
				Filename = "test",
				Extension = ".txt",
				Size = 100,
				Mail = mail
			};
			mail.Attachments.Add(attachment);

			attachment = new Attachment {
				Filename = "test",
				Extension = ".dbf",
				Size = 10*1000,
				Mail = mail
			};
			var updateLog = new UpdateLogEntity(user);
			attachment.SendLogs.Add(new AttachmentSendLog(user, attachment, updateLog));
			mail.Attachments.Add(attachment);

			session.Save(updateLog);
			session.Flush();

			File.WriteAllBytes(mail.Attachments[0].StorageFilename(config), new byte[0]);
			File.WriteAllBytes(mail.Attachments[1].StorageFilename(config), new byte[0]);

			Open("Mails?filter.Client.Id={0}", client.Id);
			AssertText("История сообщений минипочты");
			//not implemented
			//var element = Css("table.DataTable tbody tr > td:last-child");
			var cell = ((TableRow) Css("table.DataTable tbody tr")).TableCells.Last();
			Click(cell, "Показать");
			browser.WaitUntilContainsText("Вложения");
			AssertText("test.txt");
		}
	}
}