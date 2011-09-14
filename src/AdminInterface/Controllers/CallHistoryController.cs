﻿using System;
using System.Configuration;
using System.IO;
using System.Linq;
using AdminInterface.Helpers;
using AdminInterface.Models.Security;
using AdminInterface.Models.Telephony;
using AdminInterface.Security;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Common.Tools;
using Common.Web.Ui.Helpers;

namespace AdminInterface.Controllers
{
	[
		Secure(PermissionType.CallHistory),
		Helper(typeof(ViewHelper)),
	]
	public class CallHistoryController : ARSmartDispatcherController
	{
		public void Search()
		{
			var filter = new CallRecordFilter();
			if (IsPost || Request.QueryString.Keys.Cast<string>().Any(k => k.StartsWith("filter.")))
			{
				BindObjectInstance(filter, "filter");
				var calls = filter.Find();
				PropertyBag["calls"] = calls;
			}
			PropertyBag["filter"] = filter;
		}

		public void ListenCallRecord(ulong recordId)
		{
			var searchPattern = String.Format("{0}*", recordId);
			var files = Directory.GetFiles(ConfigurationManager.AppSettings["CallRecordsDirectory"], searchPattern);
			PropertyBag["recordId"] = recordId;
			if (files.Length > 0)
				PropertyBag["call"] = CallRecord.Find(recordId);
			CancelLayout();
		}

		public void GetStream(ulong recordId, int? partNumber)
		{
			CancelView();

			var searchPattern = partNumber.HasValue ? String.Format("{0}_{1}*", recordId, partNumber.Value) :
				String.Format("{0}*", recordId);
			var files = Directory.GetFiles(ConfigurationManager.AppSettings["CallRecordsDirectory"], searchPattern);

			Response.Clear();
			var filename = partNumber.HasValue ? String.Format("{0}_{1}.wav", recordId, partNumber.Value) :
				String.Format("{0}.wav", recordId);
			Response.AppendHeader("Content-Disposition", String.Format("attachment; filename=\"{0}\"", filename));
			Response.ContentType = "audio/wav";
			foreach (var track in files)
			{
				using (var fileStream = File.OpenRead(track))
					fileStream.Copy(Response.OutputStream);
			}
		}
	}
}
