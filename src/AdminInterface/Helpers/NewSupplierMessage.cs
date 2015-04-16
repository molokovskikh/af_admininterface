using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Web;
using AdminInterface.Models;
using Castle.MonoRail.Framework;

namespace AdminInterface.Helpers
{
	public class NewSupplierMessage
	{
		private string mPickUpDir;
		private const string mAttachDir = "C:\\NewSupplierEmail\\Attachments";

		public NewSupplierMessage()
		{
			mPickUpDir = Global.Config.NewSupplierMailFilePath;
		}

		/// <summary>
		/// Сформировать файл *.eml и сохранить его в директории 
		/// указанной Global.Config.NewSupplierMailFilePath.
		/// В директории может находится только один файл *.eml
		/// при повторном сохранении любые файлы с разрешением *.eml удаляются.
		/// </summary>
		public bool CreateEmlFile(DefaultValues defaults)
		{
			bool saveResult = false;
			OperationResult = null;

			if (!String.IsNullOrEmpty(mPickUpDir)) {
				MailMessage mes = new MailMessage();
				mes.From = new MailAddress("analit@analit.ru");
				mes.To.Add("supplier@supplier.ru");
				mes.SubjectEncoding = Encoding.UTF8;
				mes.IsBodyHtml = true;
				mes.Subject = defaults.NewSupplierMailSubject;
				mes.Body = defaults.NewSupplierMailText;

				string[] attachments = GetAttachmentsArray();
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

		private string[] GetAttachmentsArray()
		{
			DirectoryInfo attachDir = new DirectoryInfo(mAttachDir);
			string[] filePathArray = null;

			if (attachDir.Exists) {
				FileInfo[] files = attachDir.GetFiles();
				filePathArray = new string[files.Length];

				for (int i = 0; i < files.Length; i++) {
					filePathArray[i] = files[i].FullName;
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

		public string OperationResult { get; set; }
	}
}