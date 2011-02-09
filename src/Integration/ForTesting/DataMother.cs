using System;
using System.Collections.Generic;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Castle.ActiveRecord;
using Common.Tools;
using Common.Web.Ui.Models;
using AdminInterface.Models.Logs;
using System.Linq;

namespace Integration.ForTesting
{
	public class DataMother
	{
		public static Client TestClient(Action<Client> action = null)
		{
			Client client;
			using (var scope = new TransactionScope(OnDispose.Rollback))
			{
				var legalEntity = new LegalEntity();
				var payer = new Payer
				{
					ShortName = "test",
					ContactGroupOwner = new ContactGroupOwner(),
					JuridicalOrganizations = new List<LegalEntity> {
						legalEntity
					}
				};
				legalEntity.Payer = payer;
				client = new Client(payer) {
					Status = ClientStatus.On,
					Segment = Segment.Wholesale,
					Type = ClientType.Drugstore,
					Name = "test",
					FullName = "test",
					HomeRegion = ActiveRecordBase<Region>.Find(1UL),
					MaskRegion = 1UL,
					ContactGroupOwner = new ContactGroupOwner(),
				};
				payer.Clients = new List<Client> { client };
				if (action != null)
					action(client);

				client.Payers.Each(p => {
					p.ContactGroupOwner.Save();
					p.Save();
					p.JuridicalOrganizations.Each(l => l.Save());
				});
				client.ContactGroupOwner.Save();
				client.SaveAndFlush();
				client.Users.Each(u => u.Setup());

				client.Settings = new DrugstoreSettings(client.Id) {
					BasecostPassword = "",
					OrderRegionMask = 1UL,
				};
				client.Settings.CreateAndFlush();
				client.MaintainIntersection();

				scope.VoteCommit();
			}
			return client;
		}

		public static Client CreateTestClientWithAddress()
		{
			return TestClient(c => {
				var address = new Address {
					Client = c,
					Value = "тестовый адрес"
				};
				c.AddAddress(address);
			});
		}

		public static Client CreateTestClient(ulong maskRegion)
		{
			return TestClient();
		}

		public static Client CreateTestClientWithUser()
		{
			return TestClient(c => {
				c.AddUser(new User{ Name = "test"});
			});
		}

		public static Payer BuildPayerForBillingDocumentTest()
		{
			var client = CreateTestClientWithAddressAndUser();
			var payer = client.Payers.First();
			payer.Recipient = Recipient.Queryable.First();
			payer.Refresh();
			return payer;
		}

		public static Client CreateTestClientWithAddressAndUser()
		{
			return CreateTestClientWithAddressAndUser(1UL);
		}

		public static Client CreateTestClientWithAddressAndUser(ulong clientRegionMask)
		{
			var client = CreateTestClient(clientRegionMask);
			using (var scope = new TransactionScope(OnDispose.Rollback))
			{
				var user = new User(client) {
					Name = "test"
				};
				user.Setup(client);
				var address = new Address {
					Client = client,
					Value = "тестовый адрес"
				};
				client.AddAddress(address);
				client.Update();
				client.Users[0].Name += client.Users[0].Id;
				client.Users[0].UpdateAndFlush();
				client.Addresses[0].Value += client.Addresses[0].Id;
				client.Addresses[0].UpdateAndFlush();
				client.Name += client.Id;
				client.UpdateAndFlush();
				client.Addresses.Single().MaintainIntersection();
				scope.VoteCommit();
			}
			client.Refresh();
			return client;
		}

		public static Supplier CreateTestSupplier(Action<Supplier> action)
		{
			using (var scope = new TransactionScope(OnDispose.Rollback))
			{
				var juridicalOrganization = new LegalEntity();
				var payer = new Payer {
					ShortName = "test",
					JuridicalOrganizations = new List<LegalEntity> {
						juridicalOrganization
					}
				};
				juridicalOrganization.Payer = payer;
				payer.Save();
				juridicalOrganization.Save();
				var supplier = new Supplier {
					Payer = payer,
					HomeRegion = Region.FindAll().Last(),
					Name = "Test supplier",
					Status = ClientStatus.On
				};
				action(supplier);
				supplier.Create();
				scope.VoteCommit();
				return supplier;
			}
		}

		public static Supplier CreateTestSupplier()
		{
			return CreateTestSupplier(s => {});
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
				var document = new Document {
					ClientCode = client.Id,
					DocumentDate = DateTime.Now.AddDays(-1),
					FirmCode = supplier.Id,
					ProviderDocumentId = "123",
					Log = documentLogEntity,
					AddressId = null,
				};
				document.Create();

				var documentLine = new DocumentLine {
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

				document.Lines = new List<DocumentLine> {
					documentLine
				};
				document.SaveAndFlush();

				scope.VoteCommit();
				return document;
			}
		}

		public static UpdateLogEntity CreateTestUpdateLogEntity(Client client)
		{
			using (var scope = new TransactionScope(OnDispose.Rollback))
			{
				var updateEntity = new UpdateLogEntity {
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

		public static Client CreateClientAndUsers()
		{
			return TestClient(c => {
				c.AddUser(new User(c) {
					Name = "test"
				});
				c.AddUser(new User(c) {
					Name = "test"
				});
			});
		}

	}
}