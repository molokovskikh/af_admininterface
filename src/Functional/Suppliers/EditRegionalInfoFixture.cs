using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Functional.ForTesting;
using Integration.ForTesting;
using Test.Support.Web;
using NUnit.Framework;

namespace Functional.Suppliers
{
	public class EditRegionalInfoFixture : FunctionalFixture
	{
		[Test]
		public void OpenPageTest()
		{
			var supplier = DataMother.CreateSupplier();
			Save(supplier);
			Open(supplier);
			Click("Настройка");
			Click("Информация");
			AssertText("Информация о прайс листе");
		}
	}
}
