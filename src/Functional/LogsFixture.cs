using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework.Config;
using Common.Web.Ui.Models;
using Integration.ForTesting;
using NUnit.Framework;
using Test.Support;
using Test.Support.Logs;
using Test.Support.Suppliers;
using Test.Support.Web;
using WatiN.Core;
using WatiN.Core.Native.Windows;

namespace Functional
{
	[TestFixture]
	public class LogsFixture : WatinFixture2
	{
		[Test]
		public void ShowTest()
		{
			var client = DataMother.CreateClientAndUsers();
			var user = client.Users.First();
			var afUpdate = new UpdateLogEntity {RequestTime = DateTime.Now, User = user, UserName = user.Name};
			ActiveRecordMediator.Save(afUpdate);
			var supplier = DataMother.CreateSupplier();
			ActiveRecordMediator.Save(supplier);
			var dh = new FullDocument {Supplier = supplier, ClientCode = client.Id};
			ActiveRecordMediator.Save(dh);
			var db = dh.NewLine(new DocumentLine {Product = "TestCertificateRequestLogProduct"});
			ActiveRecordMediator.Save(db);

			var nuSert = new CertificateRequestLog {
				Line = db,
				Update = afUpdate
			};
			ActiveRecordMediator.Save(nuSert);

			Open("Main/Stat");
			var link = browser.Elements.FirstOrDefault(e => e.Parent != null && e.Parent.Id == "StatisticsTD");
			link.Click();
			AssertText("Статистика по сертификатам");
			AssertText("TestCertificateRequestLogProduct");
		}
	}
}
