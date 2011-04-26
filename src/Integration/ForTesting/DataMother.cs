using System;
using System.Collections.Generic;
using AdminInterface.Controllers;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
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
			var payer = CreatePayer();
			var client = new Client(payer) {
				Status = ClientStatus.On,
				Segment = Segment.Wholesale,
				Type = ServiceType.Drugstore,
				Name = "test",
				FullName = "test",
				HomeRegion = ActiveRecordBase<Region>.Find(1UL),
				MaskRegion = 1UL,
				ContactGroupOwner = new ContactGroupOwner(),
			};

			client.Settings = new DrugstoreSettings(client) {
				BasecostPassword = "",
				WorkRegionMask = 1UL,
				OrderRegionMask = 1UL,
			};

			payer.Clients = new List<Client> { client };
			if (action != null)
				action(client);

			client.Payers.Each(p => p.Save());
			client.SaveAndFlush();
			client.Users.Each(u => u.Setup());

			client.MaintainIntersection();

			return client;
		}

		public static Payer CreatePayer()
		{
			var legalEntity = new LegalEntity();
			var payer = new Payer {
				Name = "test",
				ContactGroupOwner = new ContactGroupOwner(),
				JuridicalOrganizations = new List<LegalEntity> {
					legalEntity
				}
			};
			legalEntity.Payer = payer;
			return payer;
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

		public static Client CreateTestClientWithUser()
		{
			return TestClient(c => {
				c.AddUser(new User {
					Name = "test",
					WorkRegionMask = c.MaskRegion,
					OrderRegionMask = c.MaskRegion
				});
			});
		}

		public static Payer BuildPayerForBillingDocumentTest()
		{
			var client = CreateTestClientWithAddressAndUser();
			var payer = client.Payers.First();
			payer.Recipient = ActiveRecordLinqBase<Recipient>.Queryable.First();
			payer.UpdateAndFlush();
			payer.Refresh();
			return payer;
		}

		public static Client CreateTestClientWithAddressAndUser()
		{
			return CreateTestClientWithAddressAndUser(1UL);
		}

		public static Client CreateTestClientWithAddressAndUser(ulong clientRegionMask)
		{
			var client = TestClient();
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
			client.Refresh();
			return client;
		}

		public static Supplier CreateTestSupplier(Action<Supplier> action)
		{
			var juridicalOrganization = new LegalEntity();
			var payer = new Payer {
				Name = "test",
				JuridicalOrganizations = new List<LegalEntity> {
					juridicalOrganization
				}
			};
			juridicalOrganization.Payer = payer;
			payer.Save();
			juridicalOrganization.Save();
			var supplier = new Supplier {
				Payer = payer,
				HomeRegion = ActiveRecordBase<Region>.FindAll().Last(),
				Name = "Test supplier",
			};
			action(supplier);
			supplier.Save();
			return supplier;
		}

		public static Supplier CreateTestSupplier()
		{
			return CreateTestSupplier(s => {});
		}

		public static DocumentReceiveLog CreateTestDocumentLog(Supplier supplier, Client client)
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
			return documentLogEntity;
		}

		public static Document CreateTestDocument(Supplier supplier, Client client, DocumentReceiveLog documentLogEntity)
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

			return document;
		}

		public static UpdateLogEntity CreateTestUpdateLogEntity(Client client)
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
			return updateEntity;
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

		public static RevisionAct GetAct()
		{
			var payer = new Payer {
				JuridicalName = "ООО \"Фармаимпекс\"",
				Recipient = new Recipient {
					FullName = "ООО \"АналитФАРМАЦИЯ\""
				}
			};

			var invoice1 = new Invoice(payer,
				Period.January,
				new DateTime(2011, 1, 10),
				new List<InvoicePart> { new InvoicePart(null, Period.December, 500, 2) }) {
					Id = 1,
				};
			var act = new Act(invoice1.Date, invoice1);

			var invoice2 = new Invoice(payer,
				Period.January,
				new DateTime(2011, 1, 20),
				new List<InvoicePart>{ new InvoicePart(null, Period.December, 1000, 1)});
			var act2 = new Act(invoice2.Date, invoice2);

			return new RevisionAct(payer,
				new DateTime(2011, 1, 1),
				new DateTime(2011, 2, 1),
				new List<Act> { act, act2 },
				new List<Payment> {
					new Payment(payer, new DateTime(2011, 1, 15), 1000)
				});
		}

		public static User CreateSupplierUser()
		{
			var supplier = CreateSupplier();
			var user = new User(supplier.Payer, supplier) {
				Login = "temporary-login"
			};
			user.Save();
			user.Login = user.Id.ToString();
			user.Update();
			return user;
		}

		public static Supplier CreateSupplier()
		{
			var payer = new Payer("Тестовый плательщик");
			payer.Save();
			return new Supplier {
				Payer = payer,
				HomeRegion = Region.Find(1UL),
				RegionMask = 1,
				Name = "Тестовый поставщик",
				FullName = "Тестовый поставщик",
				ContactGroupOwner = new ContactGroupOwner(ContactGroupType.ClientManagers)
			};
		}
	}
}