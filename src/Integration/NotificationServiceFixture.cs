using AdminInterface.Services;
using Common.Web.Ui.Models;
using Integration.ForTesting;
using NUnit.Framework;

namespace Integration
{
	[TestFixture]
	public class NotificationServiceFixture : IntegrationFixture
	{
		[Test]
		public void Send_notification_for_client_without_address()
		{
			var supplier = DataMother.CreateSupplier();
			supplier.ContactGroupOwner.Group(ContactGroupType.ClientManagers).AddContact(ContactType.Email, "kvasovtest@analit.net");
			supplier.Save();
			var client = DataMother.TestClient();

			var service = new NotificationService();
			service.NotifySupplierAboutDrugstoreRegistration(client, false);
		}
	}
}