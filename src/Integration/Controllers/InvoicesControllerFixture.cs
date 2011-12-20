using System;
using System.Linq;
using AdminInterface.Controllers;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Castle.MonoRail.TestSupport;
using Common.Web.Ui.Models;
using Integration.ForTesting;
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
			PrepareController(controller);
		}

		[Test, Ignore("НЕ запускается исполняемый файл для печати неправельный путь")]
		public void After_build_redirect_to_index()
		{
			var recipient = Recipient.Queryable.First();
			var region = Region.Queryable.First();

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
	}
}