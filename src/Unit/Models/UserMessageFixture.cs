using AdminInterface.Models;
using AdminInterface.Models.Billing;
using NUnit.Framework;

namespace Unit.Models
{
	[TestFixture]
	public class UserMessageFixture
	{
		[Test]
		public void Send_to_minimail()
		{
			var payer = new Payer("Тестовый поставщик");
			payer.Clients.Add(new Client(payer, Data.DefaultRegion){Id = 1});
			var message = new UserMessage {
				Payer = payer,
				SendToMinimail = true
			};
			Assert.That(message.To, Is.EqualTo("1@client.docs.analit.net"));
		}
	}
}