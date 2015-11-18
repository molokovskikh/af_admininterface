using AdminInterface;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Suppliers;
using NUnit.Framework;

namespace Unit.Models
{
	[TestFixture]
	public class DocumentLogFixture
	{
		[Test]
		public void Cleanup_file_name()
		{
			var log = new DocumentReceiveLog(new Supplier {
				Id = 7579,
				Name = "Надежда-Фарм Орел/Фарма Орел"
			}) {
				Address = new Address(new Client(), new LegalEntity(new Payer("тест"))) {
					Id = 2575
				},
				FileName = "test.txt",
				Id = 879,
			};
			var filename = log.GetRemoteFileName(new AppConfig {
				AptBox = "."
			});
			Assert.AreEqual(@".\2575\Waybills\879_Надежда-Фарм Орел_Фарма Орел(test).txt", filename);
		}
	}
}