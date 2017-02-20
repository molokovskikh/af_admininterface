using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using AdminInterface.Background;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Common.Tools;
using Integration.ForTesting;
using NHibernate.Linq;
using NUnit.Framework;

namespace Integration
{
	[TestFixture]
	public class BeAccountedUpdateFixture : AdmIntegrationFixture
	{
		private BeAccountedUpdate task;

		[SetUp]
		public void Setup()
		{
			task = new BeAccountedUpdate();
		}

		[TearDown]
		public void TearDown()
		{
			SystemTime.Now = () => DateTime.Now;
		}

		[Test]
		public void BeAccountedUpdate()
		{
			//подготовка данных
			var otherResult = session.Query<Account>().Where(s => s.FreePeriodEnd == SystemTime.Now().Date.AddDays(-13)).ToList();
			otherResult.Each(s => {
				s.FreePeriodEnd = SystemTime.Now().Date;
				session.Save(s);
			});

			var allResult = new List<Account>();
			var client = DataMother.CreateTestClientWithUser();
			var payer = DataMother.CreatePayerForBillingDocumentTest();
			client.Payers.Add(payer);
			session.Save(payer);
			payer.Addresses.Each(s => {
				allResult.Add(s.Accounting);
			});
			allResult.AddRange(payer.GetAccounts());
			payer = DataMother.CreatePayerForBillingDocumentTest();
			client.Payers.Add(payer);
			session.Save(payer);
			session.Save(client);
			payer.Addresses.Each(s => {
				allResult.Add(s.Accounting);
			});
			allResult.AddRange(payer.GetAccounts());
			Reopen();

			allResult.Each(s => {
				s.IsFree = true;
				s.BeAccounted = false;
				s.FreePeriodEnd = null;
				session.Save(s);
			});

			var itemCountA = (allResult.Count > 10 ? 10 : allResult.Count);
			for (int i = 0; i < itemCountA; i++) {
				allResult[i].WriteTime = SystemTime.Now().Date.AddDays(-14);
				allResult[i].FreePeriodEnd = SystemTime.Now().Date.AddDays(-13);
				allResult[i].BeAccounted = true;
				session.Save(allResult[i]);
			}

			var timeToUpdate = ConfigurationManager.AppSettings["BeAccountedUpdateAt"]
				.Split(new[] {':'}, StringSplitOptions.RemoveEmptyEntries);
			var timeToSendMailHour = int.Parse(timeToUpdate[0]);
			var timeToSendMailMinutes = timeToUpdate.Length > 1 ? int.Parse(timeToUpdate[1]) : 0;
			var mailTime = SystemTime.Now().Date.AddHours(timeToSendMailHour).AddMinutes(timeToSendMailMinutes);

			SystemTime.Now = () => mailTime.AddMinutes(10);

			Reopen();
			task.Execute();
			Reopen();

			allResult = session.Query<Account>().Where(s => s.FreePeriodEnd == SystemTime.Now().Date.AddDays(-13)).ToList();

			Assert.IsTrue(allResult.Count == itemCountA);
			for (int i = 0; i < itemCountA; i++) {
				session.Refresh(allResult[i]);
				Assert.IsTrue(allResult[i].BeAccounted == false);
			}
		}

		[Test]
		public void BeAccountedWithSumNotUpdate()
		{
			//подготовка данных
			var otherResult = session.Query<Account>().Where(s => s.FreePeriodEnd == SystemTime.Now().Date.AddDays(-13)).ToList();
			otherResult.Each(s => {
				s.FreePeriodEnd = SystemTime.Now().Date;
				session.Save(s);
			});

			var allResult = new List<Account>();
			var client = DataMother.CreateTestClientWithUser();
			var payer = DataMother.CreatePayerForBillingDocumentTest();
			client.Payers.Add(payer);
			session.Save(payer);
			payer.Addresses.Each(s => {
				allResult.Add(s.Accounting);
			});
			allResult.AddRange(payer.GetAccounts());
			payer = DataMother.CreatePayerForBillingDocumentTest();
			client.Payers.Add(payer);
			session.Save(payer);
			session.Save(client);
			payer.Addresses.Each(s => {
				allResult.Add(s.Accounting);
			});
			allResult.AddRange(payer.GetAccounts());
			Reopen();

			allResult.Each(s => {
				s.BeAccounted = false;
				s.FreePeriodEnd = null;
				session.Save(s);
			});

			var itemCountA = (allResult.Count > 10 ? 10 : allResult.Count);
			for (int i = 0; i < itemCountA; i++)
			{
				allResult[i].WriteTime = SystemTime.Now().Date.AddDays(-14);
				allResult[i].FreePeriodEnd = SystemTime.Now().Date.AddDays(-13);
				allResult[i].Payment = 50;
				allResult[i].BeAccounted = true;
				session.Save(allResult[i]);
			}

			var timeToUpdate = ConfigurationManager.AppSettings["BeAccountedUpdateAt"]
				.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
			var timeToSendMailHour = int.Parse(timeToUpdate[0]);
			var timeToSendMailMinutes = timeToUpdate.Length > 1 ? int.Parse(timeToUpdate[1]) : 0;
			var mailTime = SystemTime.Now().Date.AddHours(timeToSendMailHour).AddMinutes(timeToSendMailMinutes);

			SystemTime.Now = () => mailTime.AddMinutes(10);

			Reopen();
			task.Execute();
			Reopen();

			allResult = session.Query<Account>().Where(s => s.FreePeriodEnd == SystemTime.Now().Date.AddDays(-13)).ToList();

			Assert.IsTrue(allResult.Count == itemCountA);
			for (int i = 0; i < itemCountA; i++)
			{
				session.Refresh(allResult[i]);
				Assert.IsTrue(allResult[i].BeAccounted == true);
			}
		}
	}
}