using System;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Suppliers;
using AdminInterface.Services;
using Common.Web.Ui.Models;
using Integration.ForTesting;
using NHibernate.Linq;
using NUnit.Framework;

namespace Integration
{
	[TestFixture]
	public class NotificationServiceFixture : Test.Support.IntegrationFixture
	{
		private Supplier _supplier;
		private DefaultValues _defaults;
		private NotificationService _service;
		private Client _client;

		private string _email;

		[SetUp]
		public void SetUp()
		{
			_email = Guid.NewGuid().ToString().Replace("-", string.Empty) + "@analit.net";

			_supplier = DataMother.CreateSupplier();
			_supplier.ContactGroupOwner.Group(ContactGroupType.ClientManagers).AddContact(ContactType.Email, _email);

			var item = _supplier.Prices.First().Costs.First().PriceItem;
			item.PriceDate = DateTime.Now;
			Save(item);
			Save(_supplier);

			Flush();

			_defaults = session.Query<DefaultValues>().First();
			_service = new NotificationService(_defaults);

			_client = DataMother.TestClient();

			var emails = _service.GetEmailsForNotification(_client);
			Assert.True(emails.Contains(_email));
		}

		[Test]
		public void Send_notification_for_client_without_address()
		{
			_service.NotifySupplierAboutDrugstoreRegistration(_client, false);
		}

		[Test]
		public void Get_email_price_date_test()
		{
			var item = _supplier.Prices.First().Costs.First().PriceItem;
			item.PriceDate = DateTime.MinValue;
			Save(item);
			Flush();
			var emails = _service.GetEmailsForNotification(_client);
			Assert.IsFalse(emails.Contains(_email));
		}

		[Test]
		public void Get_email_disable_supplier_test()
		{
			_supplier.Disabled = true;
			CheckResult();
		}

		[Test]
		public void Get_email_region_mask_test()
		{
			_supplier.RegionMask = _supplier.RegionMask >> 1;
			CheckResult();
		}

		[Test]
		public void Get_email_disabled_price_test()
		{
			var price = _supplier.Prices.First();
			price.Enabled = false;
			Save(price);
			CheckResult();
		}

		[Test]
		public void Get_email_agency_enabled_price_test()
		{
			var price = _supplier.Prices.First();
			price.AgencyEnabled = false;
			Save(price);
			CheckResult();
		}

		[Test]
		public void Get_email_price_assortment_type_test()
		{
			var price = _supplier.Prices.First();
			price.PriceType = PriceType.Assortment;
			Save(price);
			CheckResult();
		}

		[Test]
		public void Get_email_price_region_test()
		{
			var region = session.Query<Region>().First(r => r.Id != 1u);
			var data = _supplier.Prices.First().RegionalData;
			foreach (var priceRegionalData in data) {
				priceRegionalData.Region = region;
				Save(priceRegionalData);
			}
			CheckResult();
		}

		private void CheckResult()
		{
			Save(_supplier);
			Flush();
			var emails = _service.GetEmailsForNotification(_client);
			Assert.IsFalse(emails.Contains(_email));
		}
	}
}