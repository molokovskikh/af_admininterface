using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using AdminInterface;
using AdminInterface.Background;
using AdminInterface.Models;
using Common.Tools;
using Common.Web.Ui.Models;
using Integration.ForTesting;
using NUnit.Framework;
using System.Net.Mail;
using Common.Web.Ui.NHibernateExtentions;
using NHibernate.Linq;

namespace Integration
{
	[TestFixture]
	public class SendPremoderatedPomotionListFixture : AdmIntegrationFixture
	{
		private SendPremoderatedPomotionList task;

		private List<MailMessage> messages;

		[SetUp]
		public void Setup()
		{
			var promotionsToClean = session.Query<SupplierPromotion>().ToList();
			session.DeleteEach(promotionsToClean);
			Flush();
			messages = new List<MailMessage>();
			ForTest.InitializeMailer();
		}

		[Test]
		public void Send_notification_aboutPremoderation()
		{
			var startCount = 5;
      var supplier = CreateSupplier();
			var catalog = FindFirstFreeCatalog();

			for (int i = 0; i < startCount; i++)
				CreatePromotion(supplier, catalog, true);
			for (int i = 0; i < startCount; i++)
				CreatePromotion(supplier, catalog, false);
			Flush();

			var promotions = session.Query<SupplierPromotion>()
			.Where(p => !p.Moderated && p.Enabled).ToList();

			var timeToSendMail = ConfigurationManager.AppSettings["SendPremoderatedPomotionListAt"]
				.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
			var timeToSendMailHour = int.Parse(timeToSendMail[0]);
			var timeToSendMailMinutes = timeToSendMail.Length > 1 ? int.Parse(timeToSendMail[1]) : 0;
			var mailTime = SystemTime.Now().Date.AddHours(timeToSendMailHour).AddMinutes(timeToSendMailMinutes);

      SystemTime.Now = () => mailTime.AddMinutes(10);
			FlushAndCommit();
			var mailer = ForTest.TestMailer(m => messages.Add(m));
			task = new SendPremoderatedPomotionList(mailer);
			task.Execute();

			var messageExists = messages.Any(m => m.Body.Contains(String.Format("Ожидают модерации {0} промо-акций:", startCount)));
			Assert.That(messageExists);
			messages.Clear();
			promotions.Each(s => { s.Moderated = true; s.UpdateStatus(); session.Update(s); });
			promotions[0].Moderated = false;
			promotions[0].UpdateStatus();
			session.Update(promotions[0]);
			Flush();

			task.Execute();
			messageExists = messages.Any(m => m.Body.Contains(String.Format("Ожидают модерации {0} промо-акций:", 1)));
			Assert.That(messageExists);
		}


		private PromotionOwnerSupplier CreateSupplier()
		{
			var supplier = DataMother.CreateSupplier();
			Save(supplier);
			supplier.Name += " " + supplier.Id;
			Save(supplier);
			Flush();
			return session.Load<PromotionOwnerSupplier>(supplier.Id);
		}

		private Catalog FindFirstFreeCatalog()
		{
			DataMother.CreateCatelogProduct();
			var catalogId = session.CreateSQLQuery(@"
select
	catalog.Id
from
	catalogs.catalog
	left join usersettings.PromotionCatalogs pc on pc.CatalogId = catalog.Id
	left join usersettings.SupplierPromotions sp on sp.Id = pc.PromotionId
	left join Customers.Suppliers s on s.Id = sp.SupplierId
where
	catalog.Hidden = 0
and s.Id is null
limit 1")
				.UniqueResult<uint>();
			return session.Load<Catalog>(catalogId);
		}

		private SupplierPromotion CreatePromotion(PromotionOwnerSupplier supplier, Catalog catalog, bool moderated)
		{
			var supplierPromotion = new SupplierPromotion {
				Enabled = true,
				PromotionOwnerSupplier = supplier,
				Annotation = catalog.Name,
				Name = catalog.Name,
				Begin = DateTime.Now.Date.AddDays(-7),
				End = DateTime.Now.Date,
				Moderated = moderated,
				Catalogs = new List<Catalog> { catalog }
			};
			supplierPromotion.UpdateStatus();
			session.Save(supplierPromotion);
			return supplierPromotion;
		}
	}
}