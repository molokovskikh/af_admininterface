using AdminInterface.Models;
using NUnit.Framework;

namespace Unit.Models
{
	[TestFixture]
	public class OrderLogFixture
	{
		[Test]
		public void Reject_message()
		{
			var log = new OrderLog {
				ResultCode = 1,
				TransportType = 999,
				Submited = true,
				Addition = "Отказ: Несуществующая пара кодов клиента"
			};

			Assert.AreEqual("Отказ: Несуществующая пара кодов клиента", log.GetResult());
		}
	}
}