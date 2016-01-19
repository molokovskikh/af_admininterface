using System;
using System.Linq;
using System.Collections.Generic;
using AdminInterface.Background;
using AdminInterface.Models;
using Common.Tools;
using Common.Web.Ui.Models;
using Common.Web.Ui.NHibernateExtentions;
using Integration.ForTesting;
using NHibernate.Util;
using NUnit.Framework;

namespace Integration.Tasks
{
	[TestFixture]
	public class WarnFixture : AdmIntegrationFixture
	{
		[Test]
		public void Warn()
		{
			var supplier = DataMother.CreateSupplier();
			var client = DataMother.CreateTestClientWithAddressAndUser();
			var user = client.Users[0];
			user.AvaliableAddresses.Add(client.Addresses[0]);
			var order = new ClientOrder(user, supplier.Prices[0]);
			order.WriteTime = DateTime.Now.AddDays(-7);
			order.Processed = true;
			var product = new Product(session.Load<Catalog>(DataMother.CreateCatelogProduct()));
			session.Flush();
			var line = new OrderLine(order, product, 100, 50);
			session.SaveMany(order, product, line);

			var messages = new Dictionary<string, string>();
			Redmine.DebugCallback = (x, y) => {
				messages.Add(x, y);
			};

			session.Clear();
			var task = new Warn(session);
			task.Execute();
			session.Flush();
			var issue = messages[messages.Keys.First(x => x.Contains($" {user.Id} "))];
			Assert.That(issue, Is.StringContaining("не обновлялся за период с"), issue);

			messages.Clear();
			SystemTime.Now = () => DateTime.Now.AddDays(7);
			task = new Warn(session);
			task.Execute();
			issue = messages[messages.Keys.First(x => x.Contains("Падение объема") && x.Contains($" {user.Id}"))];
			Assert.That(issue, Is.StringContaining("объем закупок уменьшился на 100%"), issue);
		}
	}
}