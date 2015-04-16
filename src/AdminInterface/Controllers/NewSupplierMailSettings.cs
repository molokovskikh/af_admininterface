using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using AdminInterface.MonoRailExtentions;

namespace AdminInterface.Controllers
{
	/// <summary>
	/// </summary>
	public class NewSupplierMailSettingsController : AdminInterfaceController
	{
		private const string mAttachDir = "C:\\NewSupplierEmail\\Attachments";

		public void AddAttachment()
		{
			IDictionary uploadFiles = Request.Files;
			foreach (var key in uploadFiles.Keys) {
				HttpPostedFile postedFile = uploadFiles[key] as HttpPostedFile;
				if (postedFile != null) {
					BinaryReader reader = new BinaryReader(postedFile.InputStream);
					byte[] content = reader.ReadBytes((int)postedFile.InputStream.Length);
					string fileName = Path.GetFileName(postedFile.FileName);

					if (!String.IsNullOrEmpty(fileName)) {
						string savePath = Path.Combine(mAttachDir, fileName);
						File.WriteAllBytes(savePath, content);
					}
				}
			}
			Response.Output.Write("Ok");
			CancelView();
		}

		public void DeleteAttachment()
		{
			string filListString = null;
			using (var sr = new StreamReader(Request.InputStream)) {
				filListString = sr.ReadToEnd();
			}

			if (!String.IsNullOrEmpty(filListString)) {
				filListString = filListString.Remove(filListString.Length - 1, 1);

				string[] filesPathList = filListString.Split(';');
				foreach (var filePath in filesPathList) {
					var file = new FileInfo(Path.Combine(mAttachDir, filePath));
					if (file.Exists)
						file.Delete();
				}
			}
			CancelView();
		}

		public void GetFiles()
		{
			DirectoryInfo dir = new DirectoryInfo(mAttachDir);
			string result = "Error";

			if (dir.Exists) {
				FileInfo[] files = dir.GetFiles();
				XElement rootEl = new XElement("FileList");
				int id = 0;

				foreach (FileInfo file in files)
					rootEl.Add(new XElement("File", new XElement("FileID", ++id),
						new XElement("FileName", file.Name),
						new XElement("ForDelete", false)));
				result = rootEl.ToString(SaveOptions.DisableFormatting);
			}
			Response.Output.Write(result);
			CancelView();
		}
	}
}