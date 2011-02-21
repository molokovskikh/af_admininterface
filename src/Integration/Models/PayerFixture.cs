using System;
using System.Linq;
using AdminInterface.Models;
using Integration.ForTesting;
using log4net.Config;
using NUnit.Framework;

namespace Integration.Models
{
	[TestFixture]
	public class PayerFixture : IntegrationFixture
	{
		[Test]
		public void Search_payer()
		{
			var client = DataMother.TestClient();
			var payer = client.Payers.First();
			payer.ShortName = String.Format("Тестовый поставщик {0}", payer.Id);
			payer.UpdateAndFlush();

			var payers = Payer.GetLikeAvaliable(payer.ShortName);
			Assert.That(payers.Count(), Is.EqualTo(1));
			Assert.That(payers.Single().Id, Is.EqualTo(payer.Id));
		}
	}
}