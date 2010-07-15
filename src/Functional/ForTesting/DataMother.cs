using System;
using System.Collections.Generic;
using AdminInterface.Models;
using Castle.ActiveRecord;
using Common.Tools;
using Common.Web.Ui.Models;
using AdminInterface.Models.Logs;
using System.Linq;

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
					Value = "�������� �����"
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
			return CreateTestClient(1UL);
		}

		public static Client CreateTestClient(ulong maskRegion)
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
					MaskRegion = maskRegion,
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
			return CreateTestClientWithAddressAndUser(1UL);
		}

		public static Client CreateTestClientWithAddressAndUser(ulong clientRegionMask)
		{
			Client client;
			using (var scope = new TransactionScope(OnDispose.Rollback))
			{
				client = CreateTestClient(clientRegionMask);
				var user = new User {
					Client = client,
					Name = "test"
				};
				user.Setup(client);
				client.Users = new List<User> { user };

				var address = new Address {
					Client = client,
					Value = "�������� �����"
				};
				client.Addresses = new List<Address> { address };
				client.Update();
				client.Users[0].Name += client.Users[0].Id;
				client.Users[0].UpdateAndFlush();
				client.Addresses[0].Value += client.Addresses[0].Id;
				client.Addresses[0].UpdateAndFlush();
				client.Name += client.Id;
				client.UpdateAndFlush();
				client.Addresses.Single().MaitainIntersection();
				scope.VoteCommit();
				client.Refresh();
			}
			return client;
		}

		public static Supplier CreateTestSupplier()
		{
			using (var scope = new TransactionScope(OnDispose.Rollback))
			{
				var payer = new Payer {ShortName = "test",};
				payer.Save();
				var supplier = new Supplier {
                    BillingInstance = payer,
                    HomeRegion = Region.FindAll().Last(),
                    Name = "Test supplier",
					Status = ClientStatus.On
				};
				supplier.Create();
				scope.VoteCommit();
				return supplier;
			}
		}

		public static DocumentReceiveLog CreateTestDocumentLog(Supplier supplier, Client client)
		{
			using (var scope = new TransactionScope(OnDispose.Rollback))
			{
				var documentLogEntity = new DocumentReceiveLog {
					Addition = "Test document log entity",
					ForClient = client,
					FromSupplier = supplier,
					Address = client.Addresses[0],
					DocumentSize = 1024,
					DocumentType = DocumentType.Waybill,
					FileName = "TestFile.txt",
					LogTime = DateTime.Now,
				};
				documentLogEntity.Create();
				client.Users.Select(u => new DocumentSendLog {
					Received = documentLogEntity,
					ForUser = u
				}).Each(d => d.Save());
				scope.VoteCommit();
				return documentLogEntity;
			}
		}

		public static Document CreateTestDocument(Supplier supplier, Client client, DocumentReceiveLog documentLogEntity)
		{
			using (var scope = new TransactionScope(OnDispose.Rollback))
			{
				var document = new Document
					{
						ClientCode = client.Id,
						DocumentDate = DateTime.Now.AddDays(-1),
						FirmCode = supplier.Id,
						ProviderDocumentId = "123",
						Log = documentLogEntity,
						AddressId = null,
					};
				document.Create();

				var documentLine = new DocumentLine
					{
						Certificates = "Test certificate",
						Code = "999",
						Country = "Test country",
						Nds = 10,
						Period = "01.10.2010",
						Producer = "Test producer",
						ProducerCost = 10.10M,
						VitallyImportant = true,
						Document = document,
					};
				documentLine.Create();

				document.Lines = new List<DocumentLine>();
				document.Lines.Add(documentLine);
				document.SaveAndFlush();

				scope.VoteCommit();
				return document;
			}
		}

		public static UpdateLogEntity CreateTestUpdateLogEntity(Client client)
		{
			using (var scope = new TransactionScope(OnDispose.Rollback))
			{
				var updateEntity = new UpdateLogEntity()
					{
						User = client.Users[0],
						AppVersion = 1000,
						Addition = "Test update",
						Commit = false,
						RequestTime = DateTime.Now,
						UpdateType = UpdateType.LoadingDocuments,
						UserName = client.Users[0].Name,
					};
				updateEntity.CreateAndFlush();
				scope.VoteCommit();
				return updateEntity;
			}
		}
	}
}