using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Web;
using System.Xml.Linq;
using AdminInterface.Models;
using Castle.MonoRail.Framework;
using LumiSoft.Net.Mime.vCard;

namespace AdminInterface.Helpers
{
	public class NewSupplierMessage
	{
		private readonly string mPickUpDir;
		private readonly string mAttachDir;
		protected string RecepientName;

		private enum AttachmentFileNameFormat
		{
			FullName,
			ShortName
		}

		public NewSupplierMessage(string recepientName = null)
		{
			RecepientName = recepientName;
			mPickUpDir = Global.Config.NewSupplierMailFilePath;
			if (!Directory.Exists(mPickUpDir))
				Directory.CreateDirectory(mPickUpDir);

			mAttachDir = Path.Combine(Global.Config.NewSupplierMailFilePath, "Attachments");
			if (!Directory.Exists(mAttachDir))
				Directory.CreateDirectory(mAttachDir);
		}

		/// <summary>л
		/// Сформировать файл *.eml и сохранить его в директории 
		/// указанной Global.Config.NewSupplierMailFilePath.
		/// В директории может находится только один файл *.eml
		/// при повторном сохранении любые файлы с разрешением *.eml удаляются.
		/// </summary>
		public bool CreateEmlFile(DefaultValues defaults)
		{
			bool saveResult = false;

			if (!String.IsNullOrEmpty(mPickUpDir)) {
				MailMessage mes = new MailMessage();
				mes.From = new MailAddress("farm@analit.net");
				mes.To.Add(RecepientName ?? "supplier@supplier.ru");
				mes.SubjectEncoding = Encoding.UTF8;
				mes.IsBodyHtml = true;
				mes.Subject = defaults.NewSupplierMailSubject;
				mes.Body = defaults.NewSupplierMailText;

				string[] attachments = GetAttachmentsArray(AttachmentFileNameFormat.FullName);
				foreach (string filePath in attachments)
					if (File.Exists(filePath))
						mes.Attachments.Add(new Attachment(filePath, MediaTypeNames.Application.Octet));

				DirectoryInfo dinfo = new DirectoryInfo(mPickUpDir);

				if (!dinfo.Exists)
					dinfo.Create();

				FileInfo[] files = dinfo.GetFiles("*.eml");
				foreach (var file in files)
					file.Delete();

				SmtpClient smtpClient = new SmtpClient();
				smtpClient.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
				smtpClient.PickupDirectoryLocation = mPickUpDir;
				smtpClient.Send(mes);
			}
			return saveResult;
		}

		/// <summary>
		/// Скачать сформированный файл *.eml
		/// </summary>
		public void DownLoad(IResponse response)
		{
			string emlFilePath = GetFilePath();
			if (emlFilePath != null && response != null) {
				response.Clear();
				response.ContentType = "text/plain";
				response.Charset = "UTF-8";
				response.AppendHeader("Content-Disposition", "attachment; filename=newsupplier.eml");
				response.WriteFile(emlFilePath);
			}
		}

		public void AddAttachment(byte[] content, string fileName)
		{
			if (content == null || content.Length == 0 || String.IsNullOrEmpty(fileName))
				return;

			if (Directory.Exists(mAttachDir)) {
				fileName = Path.GetFileName(fileName);
				string savePath = Path.Combine(mAttachDir, fileName);
				File.WriteAllBytes(savePath, content);
			}
		}

		public bool DeleteAttachments(params string[] attachFiles)
		{
			int errorCount = 0;
			foreach (var filePath in attachFiles) {
				var file = new FileInfo(Path.Combine(mAttachDir, filePath));
				if (!file.Exists) {
					++errorCount;
					continue;
				}

				try {
					file.Delete();
				}
				catch {
					++errorCount;
				}
			}
			return errorCount > 0;
		}

		public string GetAttachmentsList()
		{
			string[] attachments = GetAttachmentsArray(AttachmentFileNameFormat.ShortName);
			string result = null;

			if (attachments.Length > 0) {
				XElement rootEl = new XElement("FileList");
				int id = 0;

				foreach (string fileName in attachments)
					rootEl.Add(new XElement("File", new XElement("FileID", ++id),
						new XElement("FileName", fileName),
						new XElement("ForDelete", false)));
				result = rootEl.ToString(SaveOptions.DisableFormatting);
			}
			return result;
		}

		private string[] GetAttachmentsArray(AttachmentFileNameFormat format)
		{
			DirectoryInfo attachDir = new DirectoryInfo(mAttachDir);
			string[] filePathArray = null;

			if (attachDir.Exists) {
				FileInfo[] files = attachDir.GetFiles();
				filePathArray = new string[files.Length];

				for (int i = 0; i < files.Length; i++) {
					filePathArray[i] = format == AttachmentFileNameFormat.FullName ? files[i].FullName : files[i].Name;
				}
			}
			return filePathArray;
		}

		private string GetFilePath()
		{
			DirectoryInfo dir = new DirectoryInfo(mPickUpDir);
			if (dir.Exists) {
				FileInfo file = dir.GetFiles("*.eml").SingleOrDefault();
				if (file != null)
					return file.FullName;
			}
			return null;
		}
	}
}