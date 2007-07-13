using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using AdminInterface.Model;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework.Config;
using NUnit.Framework;

namespace AdminInterface.Test.IntegrationTests
{
	[TestFixture]
	public class ClientTest : BaseIntegrationTest
	{
		[Test]
		public void GetClietnForBilling()
		{
			Client client = Client.FindClietnForBilling(2575);
			//Assert.IsTrue(NHibernate.NHibernateUtil.IsInitialized(client));
			//Assert.IsTrue(NHibernate.NHibernateUtil.IsInitialized(client.BillingInstance));
			//Assert.IsTrue(NHibernate.NHibernateUtil.IsInitialized(client.BillingInstance.Clients));
			//Assert.IsTrue(NHibernate.NHibernateUtil.IsInitialized(client.ContactGroupOwner.ContactGroups));
			//foreach (ContactGroup contactGroup in client.ContactGroupOwner.ContactGroups)
			//{
			//    Assert.IsTrue(NHibernate.NHibernateUtil.IsInitialized(contactGroup.Contacts));
			//    Assert.IsTrue(NHibernate.NHibernateUtil.IsInitialized(contactGroup.Persons));
			//}
		}
	}

	public class BaseIntegrationTest
	{
		[SetUp]
		public void InitializeActiveRecord()
		{
			ActiveRecordStarter.Initialize(Assembly.GetAssembly(typeof(Client)), ActiveRecordSectionHandler.Instance);
		}
	}
}