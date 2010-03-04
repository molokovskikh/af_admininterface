using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using AdminInterface.Helpers;
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
		Layout("GeneralWithJQuery"),
	]
	public class CallHistoryController : ARSmartDispatcherController
	{
		public void ShowCallHistory(int? rowsCount, int? pageSize, int? currentPage, int? sortColumnIndex,
			DateTime? beginDate, DateTime? endDate, string searchText)
		{
			ControllerHelper.InitParameter(ref beginDate, "beginDate", DateTime.Today.AddDays(-1), PropertyBag);
			ControllerHelper.InitParameter(ref endDate, "endDate", DateTime.Today, PropertyBag);
			ControllerHelper.InitParameter(ref sortColumnIndex, "sortColumnIndex", 1, PropertyBag);
			ControllerHelper.InitParameter(ref currentPage, "currentPage", 0, PropertyBag);
			ControllerHelper.InitParameter(ref pageSize, "pageSize", 50, PropertyBag);
			PropertyBag["searchText"] = searchText;

			var callRecords = CallRecord.GetByPeriod(beginDate.Value, endDate.Value, sortColumnIndex.Value,
													 rowsCount.HasValue, currentPage.Value, pageSize.Value, searchText);
			ControllerHelper.InitParameter(ref rowsCount, "rowsCount", callRecords.Count, PropertyBag);
			PropertyBag["calls"] = callRecords;
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
