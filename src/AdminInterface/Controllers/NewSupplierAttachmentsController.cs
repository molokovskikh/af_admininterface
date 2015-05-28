using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Windows.Forms;
using System.Xml.Linq;
using AdminInterface.Helpers;
using AdminInterface.MonoRailExtentions;
using Castle.MonoRail.Framework;
using NHibernate.Hql.Ast.ANTLR;
using NPOI.SS.Formula.Functions;

namespace AdminInterface.Controllers
{
	/// <summary>
	/// </summary>
	public class NewSupplierAttachmentsController : AdminInterfaceController
	{
		public static readonly string AddAttachSuccess = "AddAttachOk";
		public static readonly string AddAttachError = "AddAttachError";
		public static readonly string GetAttahmentsError = "GetAttahmentsError";
		public static readonly string DeleteAttachmentOk = "DeleteOk";
		public static readonly string DeleteAttachmentError = "DeleteError";
		public bool TestMode = false;
		
		[AccessibleThrough(Verb.Post)]
		public void AddAttachment()
		{
			int attachCount = 0;
			IDictionary uploadFiles = Request.Files;
			foreach (var key in uploadFiles.Keys) {
				var postedFile = uploadFiles[key];
				if (postedFile != null) {
					Stream stream = GetUploadFileProperty<Stream>(postedFile, "InputStream");
					string fileName = GetUploadFileProperty<string>(postedFile, "FileName");
					
					if (stream != null && !String.IsNullOrEmpty(fileName)) {
						BinaryReader reader = new BinaryReader(stream);
						byte[] content = reader.ReadBytes((int)stream.Length);

						var message = new NewSupplierMessage();
						message.AddAttachment(content, fileName);
						++attachCount;
					}
				}
			}

			string responseString = attachCount > 0 ? AddAttachSuccess : AddAttachError;
			Response.Clear();
			Response.Output.Write(responseString);
			CancelView();
		}

		private TP GetUploadFileProperty<TP>(object uploadFile, string propName) where TP : class
		{
			TP propValue = null;
			PropertyInfo streamProp = uploadFile.GetType().GetProperty(propName);
			if (streamProp != null)
				propValue = streamProp.GetValue(uploadFile, null) as TP;
			return propValue;
		}

		[AccessibleThrough(Verb.Post)]
		public void DeleteAttachment()
		{
			string filListString = null;
			bool hasErrors = false;
			using (var sr = new StreamReader(Request.InputStream)) {
				filListString = sr.ReadToEnd();
			}

			if (!String.IsNullOrEmpty(filListString)) {
				filListString = filListString.Remove(filListString.Length - 1, 1);
				string[] filesPathList = filListString.Split(';');
				var message = new NewSupplierMessage();
				hasErrors = message.DeleteAttachments(filesPathList);
			}
			Response.Clear();
			Response.Output.Write(hasErrors ? DeleteAttachmentError : DeleteAttachmentOk);
			CancelView();
		}

		[AccessibleThrough(Verb.Get)]
		public void GetAttachmentsList()
		{
			var message = new NewSupplierMessage();
			string xmlList = message.GetAttachmentsList();

			if (xmlList == null)
				xmlList = GetAttahmentsError;

			Response.Output.Write(xmlList);
			CancelView();
		}
	}
}