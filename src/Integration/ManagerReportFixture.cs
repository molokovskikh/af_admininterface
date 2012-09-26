using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.Controllers;
using AdminInterface.Models;
using NHibernate.Linq;
using NUnit.Framework;
using Test.Support;

namespace Integration
{
	public class ManagerReportFixture : IntegrationFixture
	{
		[Test]
		public void ClientAddressFilterTest()
		{
			var filter = new ClientAddressFilter();
			filter.Find();
			filter.PageSize = filter.RowsCount;
			var results = filter.Find();
			foreach (var registrationInformation in results) {
				var address = session.Query<Address>().First(t => t.Id == registrationInformation.Id);
				if(address.Enabled)
					if(address.Client.Enabled)
						if(address.AvaliableForUsers.Count(user => user.Logs.AFTime >= DateTime.Now.AddMonths(-1)) > 0)
							Assert.Fail(String.Format("Данные адреса {0} выбираются неверно", address.Id));
			}
		}
	}
}
