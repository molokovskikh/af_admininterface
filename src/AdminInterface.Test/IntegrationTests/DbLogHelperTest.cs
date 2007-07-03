using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Web;
using AdminInterface.Helpers;
using AdminInterface.Model;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework.Config;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using NHibernate;
using NHibernate.Expression;
using NUnit.Framework;
using Rhino.Mocks;

namespace AdminInterface.Test.IntegrationTests
{
	[TestFixture]
	public class DbLogHelperTest
	{
		[SetUp]
		public void SetUp()
		{
			ActiveRecordStarter.Initialize(Assembly.GetAssembly(typeof(Client)), ActiveRecordSectionHandler.Instance);
		}

		[Test]
		public void SavePersistentWithLogParamsTest()
		{
			Client client = Client.FindFirst(Expression.Sql(""));
			if (client.Status == ClientStatus.Off)
				client.Status = ClientStatus.On;
			else
				client.Status = ClientStatus.Off;
			DateTime currentDateTime = DateTime.Now;
			
			DbLogHelper.SavePersistentWithLogParams("TestUser", "TestHost", client);

			IList<ClientLogRecord> clientLogRecords = ClientLogRecord.GetClientLogRecords(client.Id);
			ClientLogRecord clientLogRecord = null;
			foreach (ClientLogRecord record in clientLogRecords)
			{
				
				if (record.LogTime > currentDateTime 
					|| currentDateTime - record.LogTime < TimeSpan.FromSeconds(1))
				{
					clientLogRecord = record;
					break;
				}
			}
			Assert.IsNotNull(clientLogRecord);
			Assert.AreEqual(clientLogRecord.ClientStatus, client.Status);
			Assert.AreEqual(clientLogRecord.OperatorName, "TestUser");
		}
	}
}
