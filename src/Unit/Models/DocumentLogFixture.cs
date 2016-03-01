using System.IO;
using AdminInterface;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Suppliers;
using Common.Tools;
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
			Directory.CreateDirectory(@".\2575\Waybills\");
			var expectedFilename = @".\2575\Waybills\879_Надежда-Фарм Орел_Фарма Орел(test).txt";
			FileHelper.Touch(expectedFilename);
			var filename = log.GetRemoteFileName(new AppConfig {
				AptBox = "."
			});
			Assert.AreEqual(expectedFilename, filename);
		}
	}
}