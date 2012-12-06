using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.Queries;
using NUnit.Framework;
using Test.Support;
using Test.Support.Suppliers;

namespace Integration
{
	[TestFixture]
	public class FormPositionFilterFixture : IntegrationFixture
	{
		[Test]
		public void FindTest()
		{
			var supplier = TestSupplier.Create();
			var price = supplier.Prices[0];
			var itemFormat = price.Costs[0].PriceItem.Format;
			itemFormat.PriceFormat = PriceFormatType.NativeDbf;
			itemFormat.FCode = "F1";
			session.Save(itemFormat);
			session.Flush();
			session.Save(price);
			Reopen();

			var filter = new FormPositionFilter();
			filter.Session = session;
			var result = filter.Find();
			var sourceData = result.FirstOrDefault(r => ((FormPositionItem)r).PriceCode == price.Id);
			Assert.That(sourceData != null);
			Assert.That(((FormPositionItem)sourceData).FCode, Is.Not.Null);
		}
	}
}
