using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using AdminInterface.Controllers.Filters;
using AdminInterface.Extentions;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Security;
using AdminInterface.Models.Suppliers;
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
		Helper(typeof(BindingHelper)),
		Secure
	]
	public class MainController : AdminInterfaceController
	{
		public MainController()
		{
			SetARDataBinder(AutoLoadBehavior.NullIfInvalidKey);
		}

		public void Index(DateTime? from, DateTime? to, bool full = false)
		{
			RemoteServiceHelper.Try(() => { PropertyBag["expirationDate"] = ADHelper.GetPasswordExpirationDate(Admin.UserName); });

			if (from == null || to == null) {
				from = DateTime.Today;
				to = DateTime.Today;
			}

			GetStatistics(from.Value, to.Value, full);
		}

		private void GetStatistics(DateTime fromDate, DateTime toDate, bool full)
		{
			var query = new StatQuery();
			query.Full = full;
			BindObjectInstance(query, "query");

			var data = query.Load(fromDate, toDate);
			foreach (var pair in data.ToKeyValuePairs()) {
				var column = pair.Key;
				var value = pair.Value;
				value = TryToFixBrokenDateTimeValue(value);
				if (value != DBNull.Value && column.DataType == typeof(DateTime)) {
					var dateTimeValue = (DateTime)value;
					value = dateTimeValue.ToLongTimeString();
				}
				PropertyBag[column.ColumnName] = value;
			}

			Size("MaxMailSize");
			Size("AvgMailSize");
			Size("DownloadDataSize");
			Size("DownloadDocumentSize");
			InPercentOf("TotalNotSendCertificates", "TotalCertificates");
			InPercentOf("TotalSendCertificates", "TotalCertificates");
			DoConvert<double>("OrderSum", s => s.ToString("C"));

			PropertyBag["FromDate"] = fromDate;
			PropertyBag["ToDate"] = toDate;
			PropertyBag["query"] = query;
		}

		[return: JSONReturnBinder]
		public object Status()
		{
			var errorsPrices = 0;
			if (Directory.Exists(Config.ErrorFilesPath))
				errorsPrices = Directory.GetFiles(Config.ErrorFilesPath).Length;
			var priceProcessorStat = "";
			RemoteServiceHelper.RemotingCall(s => {
				var itemList = s.GetPriceItemList();
				var downloadedCount = itemList.Count(i => i.Downloaded);
				priceProcessorStat =
					$"Всего: {itemList.Length}, загруженные: {downloadedCount}, перепроводимые: {itemList.Length - downloadedCount}, Ошибок: {errorsPrices}";
			});
			var orderProcStatus = BindingHelper.GetDescription(RemoteServiceHelper
				.GetServiceStatus(Config.OrderServiceHost, Config.OrderServiceName));
			var priceProcessorStatus = BindingHelper.GetDescription(RemoteServiceHelper
				.GetServiceStatus(Config.PriceServiceHost, Config.PriceServiceName));
			return new {
				OrderProcStatus = orderProcStatus,
				IsOrderProcUnavailable = RemoteServiceHelper.IsUnavailable(orderProcStatus),
				PriceProcessorStatus = priceProcessorStatus,
				IsPriceProcessorUnavailable = RemoteServiceHelper.IsUnavailable(priceProcessorStatus),
				PriceProcessorStat = priceProcessorStat
			};
		}

		//если в mysql применить агрегирующую функцию к выражению с датой
		//то результирующий тип будет строка, правим это
		//пример max(if(1 == 1, someDate, null))
		public static object TryToFixBrokenDateTimeValue(object value)
		{
			if (value == null)
				return null;
			if (!(value is string))
				return value;
			var stringValue = (string)value;
			DateTime dateValue;
			if (!DateTime.TryParseExact(stringValue, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out dateValue))
				return value;
			return dateValue.ToLongTimeString();
		}

		public void DoConvert<T>(string key, Func<T, object> convert)
		{
			var value = PropertyBag[key];
			if (value == null || value == DBNull.Value)
				return;
			PropertyBag[key] = convert((T)Convert.ChangeType(value, typeof(T)));
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
		public void Settings(string pageTab)
		{
			var defaults = Defaults;
			NewSupplierMessage newSupplierMessage = new NewSupplierMessage();

			if (IsPost) {
				((ARDataBinder)Binder).AutoLoad = AutoLoadBehavior.Always;
				BindObjectInstance(defaults, ParamStore.Form, "defaults");
				if (IsValid(defaults)) {
					newSupplierMessage.CreateEmlFile(defaults);
					Notify("Сохранено");
					RedirectToUrl(Request.UrlReferrer + pageTab);
				}
				else {
					ActiveRecordMediator.Evict(defaults);
					PropertyBag["Defaults"] = defaults;
					PropertyBag["Formaters"] = OrderHandler.Formaters();
					PropertyBag["Senders"] = OrderHandler.Senders();
				}
			}
			else {
				PropertyBag["Defaults"] = defaults;
				PropertyBag["Formaters"] = OrderHandler.Formaters();
				PropertyBag["Senders"] = OrderHandler.Senders();
			}
		}

		public void Report(uint id, bool isPasswordChange, string passwordId)
		{
			CancelLayout();
			var user = DbSession.Load<User>(id);
			var addresses = new StringBuilder();
			if (user.RootService is Client) {
				foreach (var address in user.AvaliableAddresses)
					addresses.AppendFormat("<div>{0}</div>", address.Name);
				addresses.Append("  ");
			}

			PropertyBag["now"] = DateTime.Now;
			PropertyBag["user"] = user;
			PropertyBag["addresses"] = addresses.ToString();
			PropertyBag["IsPasswordChange"] = isPasswordChange;
			PropertyBag["defaults"] = Defaults;
			PropertyBag["password"] = Session[passwordId];
		}

		public void Stat(DateTime? from, DateTime? to)
		{
			Index(from, to, true);
		}
	}
}