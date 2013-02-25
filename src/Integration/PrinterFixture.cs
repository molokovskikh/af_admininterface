using System;
using System.Collections.Generic;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Security;
using Castle.ActiveRecord;
using Integration.ForTesting;
using Test.Support;
using log4net.Config;
using NUnit.Framework;
using AdminInterface.Helpers;

namespace Integration
{
	[TestFixture]
	public class PrinterFixture
	{
		[Test]
		public void Print()
		{
/*			var print = new Printer();
			var invoice = new Invoice{
				Payer = new Payer(),
				Recipient = new Recipient(),
				Sum = 800
			};*/
			//print.Print(MailerFixture.GetViewManager(), invoice);
		}

		[Test]
		public void GetPrintedDocuments()
		{
			Printer.Program.Main(new string[] { "name", "printer", "1, 2" });
			Assert.That(((UInt32[])Printer.Program.DocumentsForTest).Length, Is.EqualTo(2));
			Assert.That(((UInt32[])Printer.Program.DocumentsForTest)[0], Is.EqualTo(1).Or.EqualTo(2));
			Assert.That(((UInt32[])Printer.Program.DocumentsForTest)[1], Is.EqualTo(1).Or.EqualTo(2));
		}

		[Test]
		public void GetPrintedDocumentsWithWrongId()
		{
			Printer.Program.Main(new string[] { "name", "printer", "1, рст" });
			Assert.That(((UInt32[])Printer.Program.DocumentsForTest).Length, Is.EqualTo(2));
			Assert.That(((UInt32[])Printer.Program.DocumentsForTest)[0], Is.EqualTo(1).Or.EqualTo(0));
			Assert.That(((UInt32[])Printer.Program.DocumentsForTest)[1], Is.EqualTo(1).Or.EqualTo(0));
		}
	}
}