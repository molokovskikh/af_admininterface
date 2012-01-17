using System;
using System.Data;
using AdminInterface.Controllers.Filters;
using AdminInterface.Extentions;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Security;
using AdminInterface.MonoRailExtentions;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using System.Linq;

namespace AdminInterface.Controllers
{
	[
		Helper(typeof (BindingHelper)),
		Secure
	]
	public class MainController : AdminInterfaceController
	{
		public MainController()
		{
			SetBinder(new ARDataBinder());
		}

		public void Index(ulong? regioncode, DateTime? from, DateTime? to, bool full)
		{
			RemoteServiceHelper.Try(() => {
				PropertyBag["expirationDate"] = ADHelper.GetPasswordExpirationDate(Admin.UserName);
			});

			var regions = RegionHelper.GetAllRegions();
			PropertyBag["Regions"] = regions;
			PropertyBag["RegionId"] = regions.Where(region => region.Name.ToLower().Equals("все")).First().Id;

			if (regioncode == null || from == null || to == null)
			{
				regioncode = Admin.RegionMask;
				from = DateTime.Today;
				to = DateTime.Today;
			}
			GetStatistics(regioncode.Value, from.Value, to.Value, full);
		}

		private void GetStatistics(ulong regionMask, DateTime fromDate, DateTime toDate, bool full)
		{
			var query = new StatQuery();
			query.Full = full;
			BindObjectInstance(query, "query");
			
			var data = query.Load(regionMask, fromDate, toDate);
#if !DEBUG
			RemoteServiceHelper.RemotingCall(s =>
			{
				PropertyBag["FormalizationQueue"] = s.InboundFiles().Length.ToString();
			});

			//Обновлений в процессе
			RemoteServiceHelper.TryDoCall(s =>
			{
				PropertyBag["ReqHL"] = s.GetUpdatingClientCount().ToString();
			});


			PropertyBag["OrderProcStatus"] = BindingHelper.GetDescription(RemoteServiceHelper.GetServiceStatus("offdc.adc.analit.net", "OrderProcService"));
			PropertyBag["PriceProcessorMasterStatus"] = BindingHelper.GetDescription(RemoteServiceHelper.GetServiceStatus("fms.adc.analit.net", "PriceProcessorService"));
#else
			PropertyBag["OrderProcStatus"] = "";
			PropertyBag["PriceProcessorMasterStatus"] = "";
#endif

			for (var i = 0; i < data.Tables.Count; i++)
			{
				var table = data.Tables[i];
				foreach (DataColumn column in table.Columns)
				{
					object value = null;
					if (table.Rows.Count > 0)
						value = table.Rows[0][column];
					if (value != DBNull.Value && column.DataType == typeof(DateTime))
					{
						var dateTimeValue = ((DateTime) value);
						if (dateTimeValue.Date == DateTime.Today)
							value = dateTimeValue.ToLongTimeString();
					}
					PropertyBag[column.ColumnName] = value;
				}
			}

			Size("MaxMailSize");
			Size("AvgMailSize");
			Size("DownloadDataSize");
			Size("DownloadDocumentSize");
			InPercentOf("TotalNotSendCertificates", "TotalCertificates");
			InPercentOf("TotalSendCertificates", "TotalCertificates");
			DoConvert<double>("OrderSum", s => s.ToString("C"));

			PropertyBag["RegionMask"] = regionMask;
			PropertyBag["FromDate"] = fromDate;
			PropertyBag["ToDate"] = toDate;
			PropertyBag["query"] = query;
		}

		public void DoConvert<T>(string key, Func<T, object> convert)
		{
			var value = PropertyBag[key];
			if (value == null || value == DBNull.Value)
				return;
			PropertyBag[key] = convert((T)Convert.ChangeType(value, typeof (T)));
		}

		private void Size(string key)
		{
			var value = PropertyBag[key];
			if (value == null || value == DBNull.Value)
				return;
			PropertyBag[key] = ViewHelper.ConvertToUserFriendlySize(Convert.ToUInt64(value));
		}

		private void InPercentOf(string valueKey, string totalKey)
		{
			var value = PropertyBag[valueKey];
			var total = PropertyBag[totalKey];
			if (value == null || value == DBNull.Value)
				return;
			if (total == null || total == DBNull.Value)
				return;

			PropertyBag[valueKey] = String.Format("{0} ({1}%)", value, ViewHelper.InPercent(value, total));
		}

		[RequiredPermission(PermissionType.EditSettings)]
		public void Settings()
		{
			var defaults = DefaultValues.Get();
			if (IsPost)
			{
				((ARDataBinder)Binder).AutoLoad = AutoLoadBehavior.Always;
				BindObjectInstance(defaults, ParamStore.Form, "defaults");
				if (defaults.IsValid()) {
					Notify("Сохранено");
					RedirectToReferrer();
				}
				else {
					ActiveRecordMediator.Evict(defaults);
					PropertyBag["Defaults"] = defaults;
					PropertyBag["Formaters"] = OrderHandler.Formaters();
					PropertyBag["Senders"] = OrderHandler.Senders();
				}
			}
			else
			{
				PropertyBag["Defaults"] = defaults;
				PropertyBag["Formaters"] = OrderHandler.Formaters();
				PropertyBag["Senders"] = OrderHandler.Senders();
			}
		}

		public void Report(uint id, bool isPasswordChange)
		{
			CancelLayout();
			if (Session["password"] != null)
				PropertyBag["password"] = Session["password"];

			PropertyBag["now"] = DateTime.Now;
			PropertyBag["user"] = User.Find(id);
			PropertyBag["IsPasswordChange"] = isPasswordChange;
			PropertyBag["defaults"] = DefaultValues.Get();
		}

		public void Stat(ulong? regioncode, DateTime? from, DateTime? to)
		{
			Index(regioncode, from, to, true);
		}
	}
}
