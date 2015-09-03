using System;
using AdminInterface.Models.Logs;
using NUnit.Framework;

namespace Unit.Models
{
	[TestFixture]
	public class AuthorizationLogEntityFixture
	{
		[Test]
		public void App_time()
		{
			var log = new AuthorizationLogEntity {
				AFTime = new DateTime(2015, 1, 15)
			};
			Assert.AreEqual(new DateTime(2015, 1, 15), log.AppTime);
			log = new AuthorizationLogEntity {
				AFTime = new DateTime(2015, 1, 15),
				AFNetTime = new DateTime(2015, 1, 16)
			};
			Assert.AreEqual(new DateTime(2015, 1, 16), log.AppTime);
		}
	}
}