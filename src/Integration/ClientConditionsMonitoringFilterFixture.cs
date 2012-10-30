using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.ManagerReportsFilters;
using AdminInterface.Models;
using AdminInterface.Models.Suppliers;
using Common.Web.Ui.Models;
using Integration.ForTesting;
using NHibernate.Linq;
using NUnit.Framework;
using Test.Support;
using Test.Support.log4net;
using PriceType = AdminInterface.Models.Suppliers.PriceType;

namespace Integration
{
	[TestFixture]
	public class ClientConditionsMonitoringFilterFixture : IntegrationFixture
	{
		private Supplier supplier;
		private Client client;
		private ClientConditionsMonitoringFilter filter;

		[SetUp]
		public void SetUp()
		{
			supplier = DataMother.CreateSupplier();
			session.Save(supplier);
			client = DataMother.CreateTestClientWithAddressAndUser();
			session.Save(client);

			var intersectionOne = session.Query<Intersection>().Where(i => i.Price == supplier.Prices.First() && i.Client == client).ToList();

			foreach (var intersection in intersectionOne) {
				intersection.AvailableForClient = false;
				session.Save(intersection);
			}

			Flush();

			var twoClient = DataMother.CreateTestClientWithAddressAndUser();
			session.Save(twoClient);

			session.Flush();

			var price = supplier.Prices.First();
			price.AddCost();
			price.AddCost();
			price.AddCost();
			price.AddCost();
			price.AddCost();
			session.Save(price);

			var intersections = session.Query<Intersection>().Where(i => i.Client == twoClient).ToList();
			foreach (var intersection in intersections) {
				intersection.SupplierClientId = "123";
				intersection.SupplierPaymentId = "123";
				intersection.AvailableForClient = true;
				intersection.PriceMarkup = 0.5;
				intersection.Cost = price.Costs.First(c => c.BaseCost);
				foreach (var addressIntersection in intersection.Addresses) {
					addressIntersection.SupplierDeliveryId = "123";
					session.Save(addressIntersection);
				}
				session.Save(intersection);
			}

			Flush();

			filter = new ClientConditionsMonitoringFilter {
				Session = session,
				ClientId = client.Id
			};
		}

		[TearDown]
		public void Tear()
		{
			session.CreateSQLQuery(@"
update ordersendrules.smart_order_rules
set AssortimentPriceCode = null;
delete from  usersettings.pricesdata;")
				.ExecuteUpdate();
		}

		[Test]
		public void All_style_test()
		{
			var result = filter.Find();
			Assert.IsTrue(result.All(r => r.ClientCodeStyle));
			Assert.IsTrue(result.All(r => r.DeliveryStyle));
			Assert.IsTrue(result.All(r => r.PaymentCodeStyle));
			Assert.IsTrue(result.All(r => r.PriceMarkupStyle));
			Assert.IsTrue(result.All(r => r.NoPriceConnected));
			Assert.IsTrue(result.All(r => r.CostCollumn));
		}

		[Test]
		public void No_client_code_style()
		{
			foreach (var intersection in AllIntersection()) {
				intersection.SupplierClientId = string.Empty;
				session.Save(intersection);
			}
			Flush();
			var result = filter.Find();
			Assert.IsFalse(result.All(r => r.ClientCodeStyle));
			Assert.IsTrue(result.All(r => r.DeliveryStyle));
			Assert.IsTrue(result.All(r => r.PaymentCodeStyle));
			Assert.IsTrue(result.All(r => r.PriceMarkupStyle));
			Assert.IsTrue(result.All(r => r.NoPriceConnected));
			Assert.IsTrue(result.All(r => r.CostCollumn));
		}

		[Test]
		public void No_Delivery_code_style()
		{
			foreach (var intersection in session.Query<AddressIntersection>().ToList()) {
				intersection.SupplierDeliveryId = string.Empty;
				session.Save(intersection);
			}
			Flush();
			var result = filter.Find();
			Assert.IsTrue(result.All(r => r.ClientCodeStyle));
			Assert.IsFalse(result.All(r => r.DeliveryStyle));
			Assert.IsTrue(result.All(r => r.PaymentCodeStyle));
			Assert.IsTrue(result.All(r => r.PriceMarkupStyle));
			Assert.IsTrue(result.All(r => r.NoPriceConnected));
			Assert.IsTrue(result.All(r => r.CostCollumn));
		}

		[Test]
		public void No_payment_code_style()
		{
			foreach (var intersection in AllIntersection()) {
				intersection.SupplierPaymentId = string.Empty;
				session.Save(intersection);
			}
			Flush();
			var result = filter.Find();
			Assert.IsTrue(result.All(r => r.ClientCodeStyle));
			Assert.IsTrue(result.All(r => r.DeliveryStyle));
			Assert.IsFalse(result.All(r => r.PaymentCodeStyle));
			Assert.IsTrue(result.All(r => r.PriceMarkupStyle));
			Assert.IsTrue(result.All(r => r.NoPriceConnected));
			Assert.IsTrue(result.All(r => r.CostCollumn));
		}

		[Test]
		public void No_PriceMarkup_code_style()
		{
			foreach (var intersection in AllIntersection()) {
				intersection.PriceMarkup = 0f;
				session.Save(intersection);
			}
			Flush();
			var result = filter.Find();
			Assert.IsTrue(result.All(r => r.ClientCodeStyle));
			Assert.IsTrue(result.All(r => r.DeliveryStyle));
			Assert.IsTrue(result.All(r => r.PaymentCodeStyle));
			Assert.IsFalse(result.All(r => r.PriceMarkupStyle));
			Assert.IsTrue(result.All(r => r.NoPriceConnected));
			Assert.IsTrue(result.All(r => r.CostCollumn));
		}

		[Test]
		public void No_CostCollumn_code_style()
		{
			foreach (var intersection in AllIntersection()) {
				var cost = intersection.Cost;
				cost.BaseCost = false;
				session.Save(cost);
			}
			Flush();
			var result = filter.Find();
			Assert.IsTrue(result.All(r => r.ClientCodeStyle));
			Assert.IsTrue(result.All(r => r.DeliveryStyle));
			Assert.IsTrue(result.All(r => r.PaymentCodeStyle));
			Assert.IsTrue(result.All(r => r.PriceMarkupStyle));
			Assert.IsTrue(result.All(r => r.NoPriceConnected));
			Assert.IsFalse(result.All(r => r.CostCollumn));
		}

		[Test]
		public void No_NoPriceConnected_code_style()
		{
			foreach (var intersection in AllIntersection()) {
				intersection.AvailableForClient = true;
				session.Save(intersection);
			}
			Flush();
			var result = filter.Find();
			Assert.IsTrue(result.All(r => r.ClientCodeStyle));
			Assert.IsTrue(result.All(r => r.DeliveryStyle));
			Assert.IsTrue(result.All(r => r.PaymentCodeStyle));
			Assert.IsTrue(result.All(r => r.PriceMarkupStyle));
			Assert.IsFalse(result.All(r => r.NoPriceConnected));
			Assert.IsTrue(result.All(r => r.CostCollumn));
		}

		private List<Intersection> AllIntersection()
		{
			return session.Query<Intersection>().ToList();
		}
	}
}
