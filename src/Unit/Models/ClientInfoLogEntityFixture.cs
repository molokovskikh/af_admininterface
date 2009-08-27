using AdminInterface.Models;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using NUnit.Framework;


namespace AdminInterface.Test.Models
{
	[TestFixture]
	public class ClientInfoLogEntityFixture
	{
		[Test]
		public void Message_on_status_change()
		{
			SecurityContext.GetAdministrator = () => new Administrator();
			Assert.That(ClientInfoLogEntity.StatusChange(ClientStatus.Off, 2575u).Message, 
				Is.EqualTo("$$$Клиент отключен"));
			Assert.That(ClientInfoLogEntity.StatusChange(ClientStatus.On, 2575u).Message, 
				Is.EqualTo("$$$Клиент включен"));
		}
	}
}
