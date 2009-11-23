using System;
using System.Data;
using AdminInterface.Extentions;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Common.MySql;
using Common.Web.Ui.Helpers;
using MySql.Data.MySqlClient;

namespace AdminInterface.Controllers
{
	[
		Layout("General"),
		Secure
	]
	public class MainController : ARSmartDispatcherController
	{
		public void Index(ulong? regioncode, DateTime? from, DateTime? to)
		{
			SecurityContext.CheckIsUserAuthorized();

			RemoteServiceHelper.Try(() => {
				PropertyBag["expirationDate"] = ADHelper.GetPasswordExpirationDate(SecurityContext.Administrator.UserName);
			});

			PropertyBag["Regions"] = Region.GetRegionsForClient(SecurityContext.Administrator.UserName);
			PropertyBag["admin"] = SecurityContext.Administrator;

			if (regioncode == null || from == null || to == null)
			{
				regioncode = SecurityContext.Administrator.RegionMask;
				from = DateTime.Today;
				to = DateTime.Today;
			}
			GetStatistics(regioncode.Value, from.Value, to.Value);
		}

		private void GetStatistics(ulong regionMask, DateTime fromDate, DateTime toDate)
		{
			PropertyBag["RegionMask"] = regionMask;
			PropertyBag["FromDate"] = fromDate;
			PropertyBag["ToDate"] = toDate;

			var data = new DataSet();
			With.Connection(
				c =>
				{
					var adapter = new MySqlDataAdapter(@"
SELECT max(UpdateDate) MaxUpdateTime
FROM future.Clients cd
	join future.Users u on u.ClientId = cd.Id
		JOIN usersettings.UserUpdateInfo uui on uui.UserId = u.Id
WHERE   cd.RegionCode & ?RegionMaskParam > 0
        AND uui.UncommitedUpdateDate BETWEEN ?StartDateParam AND ?EndDateParam;
		 
SELECT cast(concat(count(if(resultid=2, PriceItemId, null)), '(', count(DISTINCT if(resultid=2, PriceItemId, null)), ')') as CHAR) as FormCount,
       cast(ifnull(max(if(resultid=2, logtime, null)), '2000-01-01') as CHAR) as LastForm
FROM logs.formlogs
WHERE logtime BETWEEN ?StartDateParam AND ?EndDateParam;
			 
SELECT cast(concat(count(if(resultcode=2, PriceItemId, null)), '(', count(DISTINCT if(resultcode=2, PriceItemId, null)), ')') as CHAR) as DownCount,
	   cast(ifnull(max(if(resultcode=2, logtime, null)), '2000-01-01') as CHAR) as LastDown
FROM logs.downlogs
WHERE logtime BETWEEN ?StartDateParam AND ?EndDateParam;
			 
SELECT sum(ol.cost * ol.quantity) as OrderSum,
	   count(DISTINCT oh.rowid) as TotalOrders,
	   count(DISTINCT oh.clientcode) as UniqClientOrders,
       Max(WriteTime) as MaxOrderTime
FROM orders.ordershead oh,
     orders.orderslist ol, 
     future.Clients cd, 
     usersettings.retclientsset rcs
WHERE oh.rowid = orderid
      AND cd.Id = oh.clientcode
      AND cd.PayerId <> 921
      AND rcs.clientcode = oh.clientcode
      AND cd.Segment = 0
      AND rcs.serviceclient = 0 
      AND oh.regioncode & ?RegionMaskParam   > 0
      AND oh.Deleted = 0
	  AND oh.Submited = 1
      AND WriteTime BETWEEN ?StartDateParam AND ?EndDateParam;

select count(oh.RowId) as NonProcOrdersCount
from orders.ordershead oh
where	oh.processed = 0
		and oh.submited = 1
		and oh.deleted = 0;

SELECT ifnull(sum(if(afu.UpdateType in (1,2), resultsize, 0)), 0) as DataSize,
       ifnull(sum(if(afu.UpdateType in (8), resultsize, 0)), 0) as DocSize,
       sum(if(afu.UpdateType = 6, 1, 0)) as UpdatesErr,
       cast(concat(Sum(afu.UpdateType IN (5)) ,'(' ,count(DISTINCT if(afu.UpdateType  IN (5), cd.Id, null)) ,')') as CHAR) UpdatesAD,
       cast(concat(sum(afu.UpdateType = 2) ,'(' ,count(DISTINCT if(afu.UpdateType = 2, cd.Id, null)) ,')') as CHAR) CumulativeUpdates,
       cast(concat(sum(afu.UpdateType = 1) ,'(' ,count(DISTINCT if(afu.UpdateType = 1, cd.Id, null)) ,')') as CHAR) Updates
FROM Future.Clients cd
	join Future.Users u on u.ClientId = cd.Id
	join logs.AnalitFUpdates afu on afu.UserId = u.Id
WHERE cd.maskregion & ?RegionMaskParam > 0
      AND afu.RequestTime BETWEEN ?StartDateParam AND ?EndDateParam;", c);
					adapter.SelectCommand.Parameters.AddWithValue("?StartDateParam", fromDate);
					adapter.SelectCommand.Parameters.AddWithValue("?EndDateParam", toDate.AddDays(1));
					adapter.SelectCommand.Parameters.AddWithValue("?RegionMaskParam", regionMask & SecurityContext.Administrator.RegionMask);
					adapter.Fill(data);
				});
			//Заказы
			//Количество принятых заказов
			PropertyBag["OPLB"] = String.Format("{0} ({1})",
									  data.Tables[3].Rows[0]["TotalOrders"],
									  data.Tables[3].Rows[0]["UniqClientOrders"]);
			//Сумма заказов
			PropertyBag["SumLB"] = Convert.ToDouble(data.Tables[3].Rows[0]["OrderSum"].IfDbNull(0)).ToString("C");

			//Очередь
			if (data.Tables[4].Rows[0]["NonProcOrdersCount"] != DBNull.Value)
				PropertyBag["OprLB"] = Convert.ToInt32(data.Tables[4].Rows[0]["NonProcOrdersCount"]);

			//Время последнего заказа
			if (data.Tables[3].Rows[0]["MaxOrderTime"] != DBNull.Value)
				PropertyBag["LOT"] = Convert.ToDateTime(data.Tables[3].Rows[0]["MaxOrderTime"]).ToLongTimeString();

			//Обновления
			//Запретов обновлений
			if (data.Tables[5].Rows[0]["UpdatesAD"] != DBNull.Value)
			{
				PropertyBag["ADHL"] = data.Tables[5].Rows[0]["UpdatesAD"].ToString();
			}
			//Ошибок обновлений
			if (data.Tables[5].Rows[0]["UpdatesErr"] != DBNull.Value)
			{
				PropertyBag["ErrUpHL"] = data.Tables[5].Rows[0]["UpdatesErr"].ToString();
			}
			//Кумулятивных обновлений
			if (data.Tables[5].Rows[0]["CumulativeUpdates"] != DBNull.Value)
			{
				PropertyBag["CUHL"] = data.Tables[5].Rows[0]["CumulativeUpdates"].ToString();
			}
			//Обычных обновлений
			if (data.Tables[5].Rows[0]["Updates"] != DBNull.Value)
			{
				PropertyBag["ConfHL"] = data.Tables[5].Rows[0]["Updates"].ToString();
			}
			//Время последнего обновления
			if (data.Tables[0].Rows[0]["MaxUpdateTime"] != DBNull.Value)
				PropertyBag["MaxUpdateTime"] = Convert.ToDateTime(data.Tables[0].Rows[0]["MaxUpdateTime"]).ToLongTimeString();

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

			PropertyBag["DownloadDataSize"] = ViewHelper.ConvertToUserFriendlySize(Convert.ToUInt64(data.Tables[5].Rows[0]["DataSize"]));
			PropertyBag["DownloadDocumentSize"] = ViewHelper.ConvertToUserFriendlySize(Convert.ToUInt64(data.Tables[5].Rows[0]["DocSize"]));
			//прайсы
			//Последняя формализация
			PropertyBag["FormPLB"] = Convert.ToDateTime(data.Tables[1].Rows[0]["LastForm"]).ToLongTimeString();
			//Последнее получение
			PropertyBag["DownPLB"] = Convert.ToDateTime(data.Tables[2].Rows[0]["LastDown"]).ToLongTimeString();
			//Получено прайсов
			PropertyBag["PriceDOKLB"] = data.Tables[2].Rows[0]["DownCount"].ToString();
			//Формализовано прайсов
			PropertyBag["PriceFOKLB"] = data.Tables[1].Rows[0]["FormCount"].ToString();
		}

		[RequiredPermission(PermissionType.EditSettings)]
		public void Settings()
		{
			PropertyBag["Defaults"] = DefaultValues.Get();
			PropertyBag["Formaters"] = OrderHandler.GetFormaters();
			PropertyBag["Senders"] = OrderHandler.GetSenders();
		}

		[AccessibleThrough(Verb.Post), RequiredPermission(PermissionType.EditSettings)]
		public void UpdateSettings([ARDataBind("defaults", AutoLoad = AutoLoadBehavior.Always)] DefaultValues defauls)
		{
			defauls.Update();
			Flash["Message"] = "Сохранено";
			RedirectToReferrer();
		}
	}
}
