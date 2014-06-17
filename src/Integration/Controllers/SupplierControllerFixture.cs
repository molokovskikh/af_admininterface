using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.Controllers;
using AdminInterface.Models;
using AdminInterface.Models.Suppliers;
using Common.Tools;
using Common.Web.Ui.Models;
using Integration.ForTesting;
using NHibernate.Linq;
using NUnit.Framework;

namespace Integration.Controllers
{
	[TestFixture]
	public class SupplierControllerFixture : ControllerFixture
	{
		private SuppliersController controller;
		private Supplier supplier;

		[SetUp]
		public void SetUp()
		{
			controller = new SuppliersController();
			controller.DbSession = session;
			supplier = DataMother.CreateSupplier();
			session.Save(supplier);
			Prepare(controller);
		}

		[Test]
		public void Delete_waybill_exclude_file_test()
		{
			var waybillExcludeFile = new WaybillExcludeFile("1234", supplier);
			session.Save(waybillExcludeFile);

			Flush();

			var result = controller.DeleteMask(waybillExcludeFile.Id);
			Assert.AreEqual(result, supplier.Id);
		}

		[Test]
		public void Do_not_register_duplicate_emails()
		{
			var supplier1 = DataMother.CreateSupplier();
			supplier1.WaybillSource = new WaybillSource(supplier1);
			supplier1.WaybillSource.EMailFrom = "test@analit.net";
			session.Save(supplier1);
			session.Flush();

			controller.Params["source.Emails[0]"] = "test@analit.net";
			Request.HttpMethod = "POST";
			controller.WaybillSourceSettings(supplier.Id);
			Assert.IsFalse(Response.WasRedirected);
			Assert.AreEqual("Ошибка сохранения", (controller.PropertyBag["message"] ?? "").ToString());
			Errors(supplier.WaybillSource);
			session.Clear();
		}

		[Test]
		public void Add_region()
		{
			var region = session.Query<Region>().First(r => r.Id != supplier.Id);
			Request.HttpMethod = "POST";
			controller.Params["edit.Region.Id"] = region.Id.ToString();
			controller.Params["edit.PermitedBy"] = "test";
			controller.Params["edit.RequestedBy"] = "test";
			controller.AddRegion(supplier.Id);

			Assert.AreEqual("Регион добавлен", Context.Flash["message"]);
			var rules = session.Query<ReorderSchedule>().Where(s => s.RegionalData.Region == region).ToArray();
			Assert.That(rules.Count(), Is.GreaterThan(0));
		}

		protected void Errors(object source)
		{
			controller.Validator.GetErrorSummary(source).ErrorMessages.Implode();
		}
	}
}
