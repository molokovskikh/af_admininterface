using System;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using Castle.ActiveRecord;
using Common.Web.Ui.Models;
using Functional.ForTesting;
using Integration.ForTesting;
using NUnit.Framework;
using Test.Support.Web;
using WatiN.Core.Native.Windows;

namespace Functional
{
	[TestFixture]
	public class LogsFixture : FunctionalFixture
	{
		[Test]
		public void ShowTest()
		{
			var client = DataMother.CreateClientAndUsers();
			var user = client.Users.First();
			var afUpdate = new UpdateLogEntity(user);
			var supplier = DataMother.CreateSupplier();
			var document = new FullDocument { Supplier = supplier, ClientCode = client.Id };
			var line = document.NewLine(new DocumentLine { Product = "TestCertificateRequestLogProduct" });

			var nuSert = new CertificateRequestLog {
				Line = line,
				Update = afUpdate
			};
			Save(afUpdate, supplier, document, nuSert);

			Open("Main/Stat");
			Css("#StatisticsTD a").Click();
			AssertText("Статистика по сертификатам");
			var otherRegion = session.QueryOver<Region>().List().Last();
			Css("#filter_Region_Id").SelectByValue(otherRegion.Id.ToString());
			Click("Показать");
			Assert.That(browser.Text, !Is.StringContaining("TestCertificateRequestLogProduct"));
			Css("#filter_Region_Id").SelectByValue(client.MaskRegion.ToString());
			Click("Показать");
			AssertText("TestCertificateRequestLogProduct");
		}
	}
}