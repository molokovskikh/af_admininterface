using AdminInterface.Models;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using NUnit.Framework;

namespace Unit.Models
{
	[TestFixture]
	public class ClientInfoLogEntityFixture
	{
		[Test]
		public void Message_on_status_change()
		{
			Assert.That(ClientInfoLogEntity.StatusChange(ClientStatus.Off, new Client()).Message, 
				Is.EqualTo("$$$Клиент отключен"));
			Assert.That(ClientInfoLogEntity.StatusChange(ClientStatus.On, new Client()).Message, 
				Is.EqualTo("$$$Клиент включен"));
		}
	}
}
