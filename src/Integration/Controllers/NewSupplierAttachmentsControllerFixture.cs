using System;
using System.IO;
using System.Text;
using System.Web;
using AdminInterface;
using AdminInterface.Controllers;
using Integration.ForTesting;
using NUnit.Framework;
using Rhino.Mocks;

namespace Integration.Controllers
{
	[TestFixture]
	public class NewSupplierAttachmentsControllerFixture : ControllerFixture
	{
		private NewSupplierAttachmentsController mController;
		private string mErrorMessage;
		private bool isDefaultRequest = true;

		[SetUp]
		public new void Setup()
		{
			Global.Config.NewSupplierMailFilePath = "../../../AdminInterface/Data/NewSupplierEmail";
			mController = new NewSupplierAttachmentsController();
			PrepareController(mController);
		}

		[Test]
		public void attachment_test()
		{
			const string fileName = "testattach.txt";

			if (!AddAttachment(fileName))
				Assert.Fail("Ошибка добавления вложения");

			if(!GetAttachmentList())
				Assert.Fail("Ошибка получения списка вложений");

			if (!DeleteAttachment(fileName))
				Assert.Fail("Ошибка удаления вложения");
		}

		private bool AddAttachment(string fileName)
		{
			const string fileContent = "test content";

			byte[] testString = Encoding.UTF8.GetBytes(fileContent);
			var file = MockRepository.GenerateStub<HttpPostedFileBase>();

			using (var stream = new MemoryStream()) {
				stream.Write(testString, 0, testString.Length);
				stream.Seek(0, SeekOrigin.Begin);
				file.Stub(x => x.FileName).Return(fileName);
				file.Stub(x => x.InputStream).Return(stream);

				Request.Files.Add(fileName, file);
				mController.AddAttachment();
			}
			return (NewSupplierAttachmentsController.AddAttachSuccess == Response.OutputContent);
		}

		private bool GetAttachmentList()
		{
			mController.GetAttachmentsList();
			return Response.OutputContent != NewSupplierAttachmentsController.GetAttahmentsError;
		}

		private bool DeleteAttachment(string fileName)
		{
			Request.InputStream = new MemoryStream(Encoding.UTF8.GetBytes(fileName + ";"));
			mController.DeleteAttachment();
			return Response.OutputContent == NewSupplierAttachmentsController.DeleteAttachmentOk;
		}
	}
}