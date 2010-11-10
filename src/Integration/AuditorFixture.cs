using System;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using Castle.ActiveRecord;
using Functional.ForTesting;
using NHibernate.Event;
using NUnit.Framework;

namespace Integration
{
	[TestFixture]
	public class AuditorFixture
	{
		[Test]
		public void Audit_client_changes()
		{
			Client client;
			string oldName;
			using (new SessionScope())
			{ 
				client = DataMother.CreateTestClient();
				oldName = client.Name;
				client.Name += "1";
				client.Save();
			}

			using (new SessionScope())
			{
				var logs = ClientInfoLogEntity.Queryable.Where(l => l.ClientCode == client.Id).ToList();
				Assert.That(logs[0].Message,
					Is.EqualTo(String.Format("Изменено название клиента старое значение '{0}' новое значение '{1}'", oldName, client.Name)));
			}
		}
	}
}