using System.Collections.Generic;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Security;
using Castle.ActiveRecord;
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
	}
}