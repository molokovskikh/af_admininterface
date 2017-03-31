using System;
using System.Collections.Generic;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Suppliers;
using Common.Tools;
using Common.Web.Ui.Models;
using NHibernate;
using NHibernate.Linq;
using DocumentType = AdminInterface.Models.Logs.DocumentType;

namespace Integration.ForTesting
{
	public class DataMother
	{
		private ISession session;

		public DataMother(ISession session)
		{
			this.session = session;
		}

		public Client TestClient(Action<Client> action = null)
		{
			var payer = CreatePayer();
			var homeRegion = session.Load<Region>(1UL);
			var client = new Client(payer, homeRegion) {
				Status = ClientStatus.On,
				Type = ServiceType.Drugstore,
				Name = "test",
				FullName = "test",
				HomeRegion = homeRegion,
				MaskRegion = homeRegion.Id,
				ContactGroupOwner = new ContactGroupOwner()
			};

			client.Settings.WorkRegionMask = homeRegion.Id;
			client.Settings.OrderRegionMask = homeRegion.Id;
			session.Query<DefaultValues>().First().Apply(client);

			action?.Invoke(client);

			client.Payers.Each(p => p.Clients.Add(client));

			client.Payers.Each(p => session.Save(p));
			session.Save(client);
			client.Users.Each(u => u.Setup(session));
			session.Flush();

			client.MaintainIntersection(session);

			return client;
		}

		public static Payer CreatePayer()
		{
			return new Payer("Тестовый плательщик", "Тестовое юр.лицо");
		}

		public Client CreateTestClientWithAddress()
		{
			return TestClient(c => {
				var address = new Address(c) {
					Value = "тестовый адрес"
				};
				c.AddAddress(address);
			});
		}

		public Client CreateTestClientWithUser(Region region = null)
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

		public Payer CreatePayerForBillingDocumentTest()
		{
			var client = CreateTestClientWithAddressAndUser();
			var payer = client.Payers.First();
			payer.Users.Each(u => u.Accounting.BeAccounted = true);
			payer.Addresses.Each(a => a.Accounting.BeAccounted = true);
			session.Save(client);
			payer.Recipient = session.Query<Recipient>().First();
			session.Save(payer);
			session.Flush();
			session.Refresh(payer);
			return payer;
		}

		public Client CreateTestClientWithAddressAndUser(ulong regionaMask = 1UL)
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
			user.Setup(session);
			var address = new Address(client) {
				Value = "тестовый адрес"
			};
			client.AddAddress(address);
			client.Users[0].Name += client.Users[0].Id;
			session.Save(client.Users[0]);
			client.Addresses[0].Value += client.Addresses[0].Id;
			session.Save(client.Addresses[0]);
			client.Name += client.Id;
			session.Save(client);
			session.Flush();
			client.Addresses.Single().MaintainIntersection(session);
			session.Refresh(client);
			return client;
		}

		public DocumentReceiveLog CreateTestDocumentLog(Supplier supplier, Client client)
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
			session.Save(documentLogEntity);
			client.Users.Select(u => new DocumentSendLog {
				Received = documentLogEntity,
				ForUser = u
			}).Each(x => session.Save(x));
			return documentLogEntity;
		}

		public FullDocument CreateTestDocument(Supplier supplier, Client client)
		{
			var log = new DocumentReceiveLog(supplier, client.Addresses[0]);
			session.Save(log);
			return CreateTestDocument(supplier, client, log);
		}

		public FullDocument CreateTestDocument(Supplier supplier, Client client, DocumentReceiveLog documentLogEntity)
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
			session.Save(document);

			return document;
		}

		public static UpdateLogEntity CreateTestUpdateLogEntity(Client client)
		{
			var updateEntity = new UpdateLogEntity(client.Users[0]) {
				AppVersion = 1000,
				Addition = "Test update",
				UpdateType = UpdateType.LoadingDocuments,
			};
			return updateEntity;
		}

		public Client CreateClientAndUsers()
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
				new List<InvoicePart> { new InvoicePart(null, "Мониторинг оптового фармрынка за декабрь", 1000, 1, DateTime.Now) });
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

		public User CreateSupplierUser()
		{
			return CreateSupplier().Users.Last();
		}

		public Supplier CreateSupplier(Action<Supplier> action = null)
		{
			var payer = new Payer("Тестовый плательщик");
			var homeRegion = session.Load<Region>(1UL);
			var supplier = new Supplier(homeRegion, payer) {
				Name = "Тестовый поставщик",
				FullName = "Тестовый поставщик",
				ContactGroupOwner = new ContactGroupOwner(ContactGroupType.ClientManagers)
			};
			supplier.RegionalData.Add(new RegionalData { Region = homeRegion, Supplier = supplier });
			supplier.AddPrice("Базовый");
			var user = new User(supplier.Payer, supplier);
			user.AssignDefaultPermission(session);
			user.Setup(session);
			action?.Invoke(supplier);
			return supplier;
		}

		public static string RandomInn()
		{
			var random = new Random();
			var inn = "";
			while (inn.Length < 12) {
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

		public Product Product()
		{
			var catalogName = new CatalogName("Тестовое наименование");
			var catalogForm = new CatalogForm("Тестовая форма выпуска");
			var catalog = new Catalog {
				Name = "Тестовый продукт"
			};
			var product = new Product(catalog);

			session.Save(catalogForm);
			session.Save(catalogName);
			catalog.NameId = catalogName.Id;
			catalog.FormId = catalogForm.Id;
			session.Save(catalog);
			session.Save(product);
			return product;
		}

		public Certificate Certificate(Catalog catalog, string certificateSource = null)
		{
			var serial = DateTime.Today.ToShortDateString();

			var certificate = new Certificate {
				SerialNumber = serial,
				Product = catalog
			};
			var certificateFile = new CertificateFile {
				CertificateSourceId = certificateSource,
				Extension = ".tif"
			};
			certificate.Files.Add(certificateFile);


			session.Save(certificateFile);
			session.Save(certificate);

			return certificate;
		}

		public uint CreateCatelogProduct()
		{
			return Convert.ToUInt32(session.CreateSQLQuery(@"
insert into Catalogs.CatalogNames(Name) values ('Тестовое наименование');
set @Nameid = last_insert_id();
insert into Catalogs.CatalogForms(Form) values ('Тестовая форма выпуска');
set @FormId = last_insert_id();
insert into Catalogs.Catalog(NameId, FormId) values (@NameId, @FormId);
select last_insert_id();")
				.UniqueResult());
		}

		public Supplier CreateMatrix()
		{
			var supplier = CreateSupplier(s => {
				s.Name = "Фармаимпекс";
				s.FullName = "Фармаимпекс";
				var price = s.AddPrice("Матрица", PriceType.Assortment);
				price.Matrix = new Matrix();
			});
			session.Save(supplier);
			session.Flush();
			return supplier;
		}
	}
}