using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.Controllers;
using AdminInterface.Models;
using AdminInterface.Models.Suppliers;
using Integration.ForTesting;
using NUnit.Framework;

namespace Integration.Controllers
{
	[TestFixture]
	public class SupplierControllerFixture : ControllerFixture
	{
		private SuppliersController _controller;
		private Supplier _supplier;

		[SetUp]
		public void SetUp()
		{
			_controller = new SuppliersController();
			_controller.DbSession = session;
			_supplier = DataMother.CreateSupplier();
			session.Save(_supplier);
			PrepareController(_controller);
		}

		[Test]
		public void Delete_waybill_exclude_file_test()
		{
			var waybillExcludeFile = new WaybillExcludeFile("1234", _supplier);
			session.Save(waybillExcludeFile);

			Flush();

			var result = _controller.DeleteMask(waybillExcludeFile.Id);
			Assert.AreEqual(result, _supplier.Id);
		}
	}
}
