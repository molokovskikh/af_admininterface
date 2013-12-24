using System;
using System.Linq;
using AdminInterface.Controllers;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Castle.MonoRail.TestSupport;
using Common.Web.Ui.Models;
using Integration.ForTesting;
using NHibernate.Linq;
using NUnit.Framework;

namespace Integration.Controllers
{
	[TestFixture]
	public class InvoicesControllerFixture : ControllerFixture
	{
		private InvoicesController controller;

		[SetUp]
		public void Setup()
		{
			controller = new InvoicesController();
			Prepare(controller);
		}

		[Test, Ignore("НЕ запускается исполняемый файл для печати неправельный путь")]
		public void After_build_redirect_to_index()
		{
			var recipient = session.Query<Recipient>().First();
			var region = session.Query<Region>().First();

			var invoiceDate = DateTime.Now;
			var period = invoiceDate.ToPeriod();
			controller.Build(null, invoiceDate, "");

			Assert.That(Response.RedirectedTo,
				Is.StringEnding(String.Format("Index?filter.Period.Year={0}&filter.Period.Interval={1}&filter.Region.Id={2}&filter.Recipient.Id={3}",
					period.Year,
					(int)period.Interval,
					region.Id,
					recipient.Id)));
		}

		[Test]
		public void Notify_abount_modification()
		{
			var payer = DataMother.CreatePayerForBillingDocumentTest();
			var invoice = new Invoice(payer, DateTime.Now);
			session.Save(invoice);

			var oldValue = invoice.Period.Clone();
			var newValue = new Period(DateTime.Today.Year + 1, invoice.Period.Interval);
			Request.Params.Add("invoice.Period.Year", newValue.Year.ToString());
			Request.Params.Add("invoice.Period.Interval", newValue.Interval.ToString());
			Request.HttpMethod = "POST";
			controller.Edit(invoice.Id);
			controller.SendMails();
			Assert.AreEqual(1, Emails.Count);
			var email = Emails[0];
			Assert.AreEqual("Изменен счет", Emails[0].Subject);
			Assert.That(email.Body, Is.StringContaining(String.Format("Параметр Период изменился с {0} на {1}", oldValue, newValue)));
		}
	}
}