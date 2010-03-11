using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Security;
using AdminInterface.Models.Telephony;
using AdminInterface.Security;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Helpers;

namespace AdminInterface.Controllers
{
	[
		Secure(PermissionType.CallHistory),
		Helper(typeof(ViewHelper)),
		Helper(typeof(BindingHelper)), 
		Layout("GeneralWithJQuery"),
	]
	public class CallHistoryController : ARSmartDispatcherController
	{
		public void Search()
		{
			var searchProperties = new CallSearchProperties {
					CallType = CallType.All,
					BeginDate = DateTime.Today.AddDays(-1),
					EndDate = DateTime.Today
				};
			PropertyBag["FindBy"] = searchProperties;
		}

		public void ShowCallHistory([DataBind("SearchBy")] CallSearchProperties searchProperties,
			int? rowsCount, int? pageSize, int? currentPage, int? sortColumnIndex)
		{
			if (searchProperties.BeginDate.Equals(DateTime.MinValue) || searchProperties.EndDate.Equals(DateTime.MinValue))
				searchProperties.Init();
			ControllerHelper.InitParameter(ref sortColumnIndex, "sortColumnIndex", -2, PropertyBag);
			ControllerHelper.InitParameter(ref currentPage, "currentPage", 0, PropertyBag);
			ControllerHelper.InitParameter(ref pageSize, "pageSize", 25, PropertyBag);

			var callRecords = CallRecord.Search(searchProperties, sortColumnIndex.Value, rowsCount.HasValue, currentPage.Value, pageSize.Value);
			ControllerHelper.InitParameter(ref rowsCount, "rowsCount", callRecords.Count, PropertyBag);
			PropertyBag["calls"] = callRecords;
            PropertyBag["FindBy"] = searchProperties;
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
			CancelLayout();
			CancelView();

			var searchPattern = partNumber.HasValue ? String.Format("{0}_{1}*", recordId, partNumber.Value) :
				String.Format("{0}*", recordId);			
			var files = Directory.GetFiles(ConfigurationManager.AppSettings["CallRecordsDirectory"], searchPattern);

			Response.Clear();
			var filename = partNumber.HasValue ? String.Format("{0}_{1}.wav", recordId, partNumber.Value) :
				String.Format("{0}.wav", recordId);
			Response.AppendHeader("Content-Disposition", String.Format("attachment; filename=\"{0}\"", filename));
			Response.ContentType = "audio/wav";
			var buffer = new byte[32768];
			foreach (var track in files)
			{
				using (var fileStream = File.OpenRead(track))
				{
					var readBytes = 0;
					do
					{
						readBytes = fileStream.Read(buffer, 0, buffer.Length);
						if (readBytes > 0)
							Response.OutputStream.Write(buffer, 0, readBytes);
					} while (readBytes > 0);
				}
			}
		}
	}
}
