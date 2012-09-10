using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.Models;
using NUnit.Framework;
using Test.Support.Web;
using WatiN.Core;

namespace Functional
{
	[TestFixture]
	class StatusColorFixture : WatinFixture2
	{
		private const string url = "DebugStatusColor/DebugStatusColor?OrderProcStatus={0}&PriceProcessorMasterStatus={1}";
		private string OrderProcStatus;
		private string PriceProcessorMasterStatus;

		[Test]
		public void UnknownStatusColorTest()
		{
			OrderProcStatus = "Недоступна";
			PriceProcessorMasterStatus = "Недоступна";
			Open(url, new object[] { OrderProcStatus, PriceProcessorMasterStatus });
			Assert.AreEqual(browser.Element(Find.ById("OrderProcStatus")).Style.BackgroundColor.ToHexString.ToUpper(), "#FF0000");
			Assert.AreEqual(browser.Element(Find.ById("PriceProcessorMasterStatus")).Style.BackgroundColor.ToHexString.ToUpper(), "#FF0000");
		}

		[Test]
		public void NotRunnigStatusColorTest()
		{
			OrderProcStatus = "Не запущена";
			PriceProcessorMasterStatus = "Не запущена";
			Open(url, new object[] { OrderProcStatus, PriceProcessorMasterStatus });
			Assert.AreEqual(browser.Element(Find.ById("OrderProcStatus")).Style.BackgroundColor.ToHexString.ToUpper(), "#FF0000");
			Assert.AreEqual(browser.Element(Find.ById("PriceProcessorMasterStatus")).Style.BackgroundColor.ToHexString.ToUpper(), "#FF0000");
		}
	}
}
