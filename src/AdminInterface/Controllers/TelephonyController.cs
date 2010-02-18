using System;
using System.Configuration;
using System.Net;
using AdminInterface.Helpers;
using AdminInterface.Models.Security;
using AdminInterface.Models.Telephony;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Helpers;
using NHibernate.Criterion;
using ViewHelper=AdminInterface.Helpers.ViewHelper;
using System.IO;
using AdminInterface.Properties;

namespace AdminInterface.Controllers
{
	[
		Secure(PermissionType.ManageCallbacks),
		Helper(typeof(ViewHelper)),
		Layout("GeneralWithJQuery"),
	]
	public class TelephonyController : ARSmartDispatcherController
	{
		public void Show()
		{
			PropertyBag["callbacks"] = Callback.FindAll(Order.Asc("Comment"));
		}

        public void UpdateCallbacks([ARDataBind("callbacks", AutoLoad = AutoLoadBehavior.Always)] Callback[] callbacks)
        {
            using (new TransactionScope())
            {
                DbLogHelper.SetupParametersForTriggerLogging<Callback>(SecurityContext.Administrator.UserName,
                                                                       Request.UserHostAddress);
                foreach (var callback in callbacks)
                    callback.Save();
            }

            Flash["isUpdated"] = true;
            RedirectToAction("Show");
        }

	    public void Update([DataBind("callback")] Callback callback)
		{
	        using (new TransactionScope())
	        {
	            DbLogHelper.SetupParametersForTriggerLogging<Callback>(SecurityContext.Administrator.UserName,
	                                                                   Request.UserHostAddress);
                callback.Save();
	        }
			
	        Flash["isUpdated"] = true;
			RedirectToAction("Show");
		}

		public void Edit(uint id)
		{
			PropertyBag["callback"] = Callback.Find(id);
		}

		public void New()
		{
			PropertyBag["callback"] = new Callback();
			RenderView("Edit");
		}

		public void Delete(uint id)
		{
		    using (new TransactionScope())
		    {
                DbLogHelper.SetupParametersForTriggerLogging<Callback>(SecurityContext.Administrator.UserName,
                                                                       Request.UserHostAddress);
                Callback.Find(id).Delete();		        
		    }
			RedirectToAction("Show");
		}

		public void ShowCallHistory(int? rowsCount, int? pageSize, int? currentPage, int? sortColumnIndex, DateTime? beginDate, DateTime? endDate)
		{
			if (!SecurityContext.Administrator.HavePermision(PermissionType.CallHistory))
				throw new NotHavePermissionException();
			ControllerHelper.InitParameter(ref beginDate, "beginDate", DateTime.Today.AddDays(-1), PropertyBag);
			ControllerHelper.InitParameter(ref endDate, "endDate", DateTime.Today, PropertyBag);
			ControllerHelper.InitParameter(ref sortColumnIndex, "sortColumnIndex", 1, PropertyBag);
			ControllerHelper.InitParameter(ref currentPage, "currentPage", 0, PropertyBag);
			ControllerHelper.InitParameter(ref pageSize, "pageSize", 50, PropertyBag);

			var callRecords = CallRecord.GetByPeriod(beginDate.Value, endDate.Value, sortColumnIndex.Value,
			                                         rowsCount.HasValue, currentPage.Value, pageSize.Value);
			ControllerHelper.InitParameter(ref rowsCount, "rowsCount", callRecords.Count, PropertyBag);
			PropertyBag["calls"] = callRecords;
		}

		public void UpdateCallHistory([DataBind("calls")] CallRecord[] calls,
			DateTime? beginDate, DateTime? endDate, int? rowsCount, int? pageSize, int? currentPage, int? sortColumnIndex)
		{
			if (!SecurityContext.Administrator.HavePermision(PermissionType.CallHistory))
				throw new NotHavePermissionException();

			RedirectToAction("ShowCallHistory", new {
				beginDate,
				endDate,
				rowsCount,
				pageSize,
				currentPage,
				sortColumnIndex
			});
		}

		public void ListenCallRecord(ulong recordId)
		{
			if (!SecurityContext.Administrator.HavePermision(PermissionType.CallHistory))
				throw new NotHavePermissionException();
			var searchPattern = String.Format("{0}*", recordId);
			var files = Directory.GetFiles(ConfigurationManager.AppSettings["CallRecordsDirectory"], searchPattern);
			if (files.Length > 0)
				PropertyBag["tracks"] = files;
			PropertyBag["recordId"] = recordId;
			CancelLayout();
		}

		public void GetStream(ulong recordId)
		{
			if (!SecurityContext.Administrator.HavePermision(PermissionType.CallHistory))
				throw new NotHavePermissionException();
			CancelLayout();
			CancelView();

			var searchPattern = String.Format("{0}*", recordId);
			var files = Directory.GetFiles(ConfigurationManager.AppSettings["CallRecordsDirectory"], searchPattern);

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
