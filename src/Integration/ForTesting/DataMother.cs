using System;
using System.Collections.Generic;
using AdminInterface.Controllers;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Common.Tools;
using Common.Web.Ui.ActiveRecordExtentions;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using AdminInterface.Models.Logs;
using System.Linq;
using Test.Support.log4net;
using DocumentType = AdminInterface.Models.Logs.DocumentType;

namespace Integration.ForTesting
{
	public class DataMother
	{
		public static Client TestClient(Action<Client> action = null)
		{
			var payer = CreatePayer();
			var homeRegion = ActiveRecordBase<Region>.Find(1UL);
			var client = new Client(payer, homeRegion) {
				Status = ClientStatus.On,
				Type = ServiceType.Drugstore,
				Name = "test",
				FullName = "test",
				HomeRegion = homeRegion,
				MaskRegion = homeRegion.Id,
				ContactGroupOwner = new ContactGroupOwner(),
			};

			client.Settings.WorkRegionMask = homeRegion.Id;
			client.Settings.OrderRegionMask = homeRegion.Id;

			payer.Clients = new List<Client> { client };
			if (action != null)
				action(client);

			client.Payers.Each(p => p.Save());
			ActiveRecordMediator.SaveAndFlush(client);
			client.Users.Each(u => u.Setup());

			client.MaintainIntersection();

			return client;
		}

		public static Payer CreatePayer()
		{
			return new Payer("Тестовый плательщик", "Тестовое юр.лицо");
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

		public static Client CreateTestClientWithUser(Region region = null)
		{
			return TestClient(c => {
				if (region != null)
					c.ChangeHomeRegion(region);
				var user = new User(c) {
					Name = "test",
				};
				c.AddUser(user);
			});
		}

		public static Payer CreatePayerForBillingDocumentTest()
		{
			var client = CreateTestClientWithAddressAndUser();
			var payer = client.Payers.First();
			payer.Users.Each(u => u.Accounting.BeAccounted = true);
			payer.Addresses.Each(a => a.Accounting.BeAccounted = true);
			ActiveRecordMediator.Save(client);
			payer.Recipient = ActiveRecordLinqBase<Recipient>.Queryable.First();
			payer.SaveAndFlush();
			payer.Refresh();
			return payer;
		}

		public static Client CreateTestClientWithAddressAndUser()
		{
			return CreateTestClientWithAddressAndUser(1UL);
		}

		public static Client CreateTestClientWithAddressAndUser(ulong regionaMask)
		{
			var client = TestClient(c => {
				c.MaskRegion = regionaMask;
				c.Settings.WorkRegionMask = regionaMask;
				c.Settings.OrderRegionMask = regionaMask;
			});
			var user = new User(client) {
				Name = "test"
			};
			client.AddUser(user);
			user.Setup();
			var address = new Address {
				Client = client,
				Value = "тестовый адрес"
			};
			client.AddAddress(address);
			client.Users[0].Name += client.Users[0].Id;
			ActiveRecordMediator.Save(client.Users[0]);
			client.Addresses[0].Value += client.Addresses[0].Id;
			client.Addresses[0].Save();
			client.Name += client.Id;
			ActiveRecordMediator.SaveAndFlush(client);
			client.Addresses.Single().MaintainIntersection();
			ActiveRecordMediator<Client>.Refresh(client);
			return client;
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
			ActiveRecordMediator.Create(documentLogEntity);
			client.Users.Select(u => new DocumentSendLog {
				Received = documentLogEntity,
				ForUser = u
			}).Each(ActiveRecordMediator.Save);
			return documentLogEntity;
		}

		public static FullDocument CreateTestDocument(Supplier supplier, Client client, DocumentReceiveLog documentLogEntity)
		{
			var document = new FullDocument {
				ClientCode = client.Id,
				DocumentDate = DateTime.Now.AddDays(-1),
				Supplier = supplier,
				ProviderDocumentId = "123",
				Log = documentLogEntity,
				AddressId = null,
			};

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

			document.Lines = new List<DocumentLine> {
				documentLine
			};
			ActiveRecordMediator.Save(document);

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
				new Period(2011, Interval.January),
				new DateTime(2011, 1, 10),
				new List<InvoicePart> { new InvoicePart(null, "Мониторинг оптового фармрынка за декабрь", 500, 2, DateTime.Now) }) {
					Id = 1,
				};
			var act = new Act(invoice1.Date, invoice1);

			var invoice2 = new Invoice(payer,
				new Period(2011, Interval.January),
				new DateTime(2011, 1, 20),
				new List<InvoicePart> { new InvoicePart(null, "Мониторинг оптового фармрынка за декабрь", 1000, 1, DateTime.Now)});
			var act2 = new Act(invoice2.Date, invoice2);

			return new RevisionAct(payer,
				new DateTime(2011, 1, 1),
				new DateTime(2011, 2, 1),
				new List<Act> { act, act2 },
				new List<Payment> {
					new Payment(payer, new DateTime(2011, 1, 15), 1000)
				},
				Enumerable.Empty<BalanceOperation>());
		}

		public static User CreateSupplierUser()
		{
			var supplier = CreateSupplier();
			var user = new User(supplier.Payer, supplier) {
				Login = User.GetTempLogin()
			};
			ActiveRecordMediator.Save(user);
			user.Setup();
			user.SetupSupplierPermission();
			ActiveRecordMediator.Save(user);
			return user;
		}

		public static Supplier CreateSupplier(Action<Supplier> action = null)
		{
			var payer = new Payer("Тестовый плательщик");
			var homeRegion = ActiveRecordBase<Region>.Find(1UL);
			var supplier = new Supplier(homeRegion, payer) {
				Name = "Тестовый поставщик",
				FullName = "Тестовый поставщик",
				ContactGroupOwner = new ContactGroupOwner(ContactGroupType.ClientManagers)
			};
			supplier.RegionalData.Add(new RegionalData{Region = homeRegion, Supplier = supplier});
			supplier.AddPrice("Базовый", PriceType.Regular);
			if (action != null)
				action(supplier);
			return supplier;
		}

		public static string RandomInn()
		{
			var random = new Random();
			var inn = "";
			while (inn.Length < 12)
			{
				inn += random.Next(0, 9);
			}
			return inn;
		}

		public static ReportAccount Report(Payer payer)
		{
			var report = new Report {
				Allow = true,
				Comment = "тестовый отчет",
				Payer = payer,
			};
			var account = new ReportAccount(report);
			return account;
		}

		public static Product Product()
		{
			var catalogName = new CatalogName("Тестовое наименование");
			var catalogForm = new CatalogForm("Тестовая форма выпуска");
			var catalog = new Catalog {
				Name = "Тестовый продукт"
			};
			var product = new Product(catalog);

			ActiveRecordMediator.Save(catalogForm);
			ActiveRecordMediator.SaveAndFlush(catalogName);
			catalog.NameId = catalogName.Id;
			catalog.FormId = catalogForm.Id;
			ActiveRecordMediator.Save(catalog);
			ActiveRecordMediator.Save(product);
			return product;
		}

		public static Certificate Certificate(Catalog catalog)
		{
			var serial = DateTime.Today.ToShortDateString();

			var certificate = new Certificate {
				SerialNumber = serial,
				Product = catalog
			};
			var certificateFile = new CertificateFile {
				Extension = ".tif"
			};
			certificate.Files.Add(certificateFile);


			ActiveRecordMediator.Save(certificateFile);
			ActiveRecordMediator.Save(certificate);

			return certificate;
		}

		public static uint CreateCatelogProduct()
		{
			return ArHelper.WithSession(s => {
				return Convert.ToUInt32(s.CreateSQLQuery(@"
insert into Catalogs.CatalogNames(Name) values ('Тестовое наименование');
set @Nameid = last_insert_id();
insert into Catalogs.CatalogForms(Form) values ('Тестовая форма выпуска');
set @FormId = last_insert_id();
insert into Catalogs.Catalog(NameId, FormId) values (@NameId, @FormId);
select last_insert_id();")
					.UniqueResult());
			});
		}
	}
}
