using AdminInterface.Models;
using AdminInterface.Models.Logs;
using NUnit.Framework;

namespace Unit.Models
{
	[TestFixture]
	public class ClientInfoLogEntityFixture
	{
		[Test]
		public void Message_on_status_change()
		{
			Assert.That(AuditRecord.StatusChange(new Client { Status = ClientStatus.Off }).Message,
				Is.EqualTo("$$$Клиент отключен"));
			Assert.That(AuditRecord.StatusChange(new Client { Status = ClientStatus.On }).Message,
				Is.EqualTo("$$$Клиент включен"));
		}
	}
}