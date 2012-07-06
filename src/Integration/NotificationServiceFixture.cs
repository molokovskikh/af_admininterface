using System.Linq;
using AdminInterface.Models;
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
		[Test]
		public void Send_notification_for_client_without_address()
		{
			var supplier = DataMother.CreateSupplier();
			supplier.ContactGroupOwner.Group(ContactGroupType.ClientManagers).AddContact(ContactType.Email, "kvasovtest@analit.net");
			Save(supplier);
			var client = DataMother.TestClient();
			var defaults = session.Query<DefaultValues>().First();

			var service = new NotificationService(defaults);
			service.NotifySupplierAboutDrugstoreRegistration(client, false);
		}
	}
}