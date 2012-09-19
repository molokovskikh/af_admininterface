using System;
using AdminInterface.Models;
using AdminInterface.Helpers;
using NUnit.Framework;

namespace Functional
{
	[TestFixture]
	public class StatusColorFixture
	{
		private const string AllNotRunnigOrUnknownStatus = "order-proc-not-runnig-or-unknown price-processor-master-not-runnig-or-unknown";
		private const string OrderProcNotRunnigOrUnknownStatus = "order-proc-not-runnig-or-unknown";
		private const string PriceProcessorMasterNotRunnigOrUnknownStatus = "price-processor-master-not-runnig-or-unknown";
		private AppHelper helper;
		private StatusServices statuses;
		private string expected;

		[SetUp]
		public void Setup()
		{
			helper = new AppHelper();
			statuses = new StatusServices();
		}

		[Test]
		public void UnknownStatusColorTest()
		{
			statuses.OrderProcStatus = "Недоступна";
			statuses.PriceProcessorMasterStatus = "";
			expected = helper.Style(statuses);
			Assert.AreEqual(expected, OrderProcNotRunnigOrUnknownStatus);

			statuses.OrderProcStatus = "";
			statuses.PriceProcessorMasterStatus = "Недоступна";
			expected = helper.Style(statuses);
			Assert.AreEqual(expected, PriceProcessorMasterNotRunnigOrUnknownStatus);

			statuses.OrderProcStatus = "Недоступна";
			statuses.PriceProcessorMasterStatus = "Недоступна";
			expected = helper.Style(statuses);
			Assert.AreEqual(expected, AllNotRunnigOrUnknownStatus);
		}

		[Test]
		public void NotRunnigStatusColorTest()
		{
			statuses.OrderProcStatus = "Не запущена";
			statuses.PriceProcessorMasterStatus = "";
			expected = helper.Style(statuses);
			Assert.AreEqual(expected, OrderProcNotRunnigOrUnknownStatus);

			statuses.OrderProcStatus = "";
			statuses.PriceProcessorMasterStatus = "Не запущена";
			expected = helper.Style(statuses);
			Assert.AreEqual(expected, PriceProcessorMasterNotRunnigOrUnknownStatus);

			statuses.OrderProcStatus = "Не запущена";
			statuses.PriceProcessorMasterStatus = "Не запущена";
			expected = helper.Style(statuses);
			Assert.AreEqual(expected, AllNotRunnigOrUnknownStatus);
		}
	}
}
