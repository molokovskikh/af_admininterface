using System.Collections.Generic;
using AdminInterface.Models;
using Castle.ActiveRecord;
using Common.Web.Ui.Models;

namespace Functional.ForTesting
{
	public class DataMother
	{
		public static Client CreateTestClientWithAddress()
		{
			using (var scope = new TransactionScope(OnDispose.Rollback))
			{
				var client = CreateTestClient();
				var address = new Address {
					Client = client,
					Value = "тестовый адрес"
				};
				client.Addresses = new List<Address> {
					address
				};
				client.Update();
				scope.VoteCommit();
				return client;
			}
		}

		public static Client CreateTestClient(Client client)
		{
			using(var scope = new TransactionScope(OnDispose.Rollback))
			{
				var payer = new Payer {
					ShortName = "test",
				};
				payer.Save();
				var contactOwner = new ContactGroupOwner();
				contactOwner.Save();

				client.Status = ClientStatus.On;
				client.Segment = Segment.Wholesale;
				client.Type = ClientType.Drugstore;
				if (client.Name == null)
					client.FullName = "test";
				if (client.FullName == null)
					client.FullName = "test";
				if (client.HomeRegion == null)
					client.HomeRegion = ActiveRecordBase<Region>.Find(1UL);
				if (client.MaskRegion == 0)
					client.MaskRegion = 1UL;
				client.BillingInstance = payer;
				client.ContactGroupOwner = contactOwner;
				client.SaveAndFlush();
				var drugstoreSettings = new DrugstoreSettings(client.Id) {
					BasecostPassword = "",
				};
				drugstoreSettings.CreateAndFlush();
				client.MaintainIntersection();

				scope.VoteCommit();
			}
			return client;
		}

		public static Client CreateTestClient()
		{
			Client client;
			using(var scope = new TransactionScope(OnDispose.Rollback))
			{
				var contactOwner = new ContactGroupOwner();
				contactOwner.Save();
				var payer = new Payer {
					ShortName = "test",
					ContactGroupOwner = contactOwner,
				};
				payer.Save();
				contactOwner = new ContactGroupOwner();
				contactOwner.Save();
				client = new Client {
					Status = ClientStatus.On,
					Segment = Segment.Wholesale,
					Type = ClientType.Drugstore,
					Name = "test",
					FullName = "test",
					HomeRegion = ActiveRecordBase<Region>.Find(1UL),
					MaskRegion = 1UL,
					BillingInstance = payer,
					ContactGroupOwner = contactOwner,
				};
				client.SaveAndFlush();
				var drugstoreSettings = new DrugstoreSettings(client.Id) {
					BasecostPassword = "",
					OrderRegionMask = 1UL,
				};
				drugstoreSettings.CreateAndFlush();
				client.MaintainIntersection();

				scope.VoteCommit();
			}
			return client;
		}

		public static Client CreateTestClientWithUser()
		{
			var client = CreateTestClient();
			var user = new User {
				Client = client,
				Name = "test"
			};
			user.Setup(client);
			client.Users = new List<User> {user};
			return client;
		}

		public static Client CreateTestClientWithAddressAndUser()
		{
			using (var scope = new TransactionScope(OnDispose.Rollback))
			{
				var client = CreateTestClient();
				var user = new User {
					Client = client,
					Name = "test"
				};
				user.Setup(client);
				client.Users = new List<User> { user };

				var address = new Address {
                    Client = client,
                    Value = "тестовый адрес"
				};
				client.Addresses = new List<Address> { address };
				client.Update();
				scope.VoteCommit();
				return client;
			}
		}
	}
}