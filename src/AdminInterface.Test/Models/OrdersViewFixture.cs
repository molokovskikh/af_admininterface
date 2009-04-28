using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.Models;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using AdminInterface.Test.ForTesting;
using Castle.ActiveRecord;
using NUnit.Framework;

namespace AdminInterface.Test.Models
{
	[TestFixture]
	public class OrdersViewFixture
	{
		[SetUp]
		public void Setup()
		{
			ForTest.InitialzeAR();			
			SecurityContext.GetAdministrator = () => new Administrator { RegionMask = ulong.MaxValue };
		}

		[Test]
		public void Find_not_sended_orders()
		{
			OrderView.FindNotSendedOrders();
		}
	}
}
