using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Integration.ForTesting;
using Test.Support.Web;
using NUnit.Framework;

namespace Functional.Suppliers
{
	public class EditRegionalInfoFixture : WatinFixture2
	{
		[Test]
		public void OpenPageTest()
		{
			var supplier = DataMother.CreateSupplier();
			Save(supplier);
			Flush();
			Open(supplier);
			Click("Настройка");
			Click("Информация");
			AssertText("Информация о прайс листе");
		}
	}
}
