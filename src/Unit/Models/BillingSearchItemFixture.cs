/*
using AdminInterface.Models;
using AdminInterface.Test.ForTesting;
using NUnit.Framework;

namespace AdminInterface.Test.Models
{
	[TestFixture]
	public class BillingSearchItemFixture
	{
		public BillingSearchItemFixture()
		{
			ForTest.InitialzeAR();
		}

		[Test]
		[Ignore("не доделано")]
		public void FindBy2()
		{
			var properties = new BillingSearchProperties
			                 	{
			                 		SearchText = "тест",
									PayerState = PayerStateFilter.Debitors,
			                 	};
			BillingSearchItem.FindBy2(properties);
		}

		[Test]
		[Ignore("не доделано")]
		public void FindBy_billingCode()
		{
			var properties = new BillingSearchProperties
			                 	{
			                 		SearchText = "13",
			                 		PayerState = PayerStateFilter.Debitors,
			                 		SearchBy = SearchBy.BillingCode,
			                 	};
			BillingSearchItem.FindBy2(properties);
		}

		[Test]
		[Ignore("не доделано")]
		public void FindBy_code()
		{
			var properties = new BillingSearchProperties
			                 	{
			                 		SearchText = "13",
			                 		PayerState = PayerStateFilter.Debitors,
			                 		SearchBy = SearchBy.Code,
			                 	};
			BillingSearchItem.FindBy2(properties);
		}

		[Test]
		[Ignore("не доделано")]
		public void FindBy_name()
		{
			var properties = new BillingSearchProperties
			{
				SearchText = "тест",
				PayerState = PayerStateFilter.Debitors,
				SearchBy = SearchBy.Name,
			};
			BillingSearchItem.FindBy2(properties);
		}
	}
}
*/
