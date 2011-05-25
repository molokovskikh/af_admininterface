using System;
using System.Data;
using AdminInterface.Extentions;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Security;
using AdminInterface.MonoRailExtentions;
using AdminInterface.Security;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Common.MySql;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using MySql.Data.MySqlClient;
using System.Linq;

namespace AdminInterface.Controllers
{
	[
		Helper(typeof(BindingHelper)),
		Layout("Main"),
		Secure
	]
	public class MainController : ARController
	{
		public void Index(ulong? regioncode, DateTime? from, DateTime? to)
		{
			RemoteServiceHelper.Try(() => {
				PropertyBag["expirationDate"] = ADHelper.GetPasswordExpirationDate(Administrator.UserName);
			});

			var regions = RegionHelper.GetAllRegions();
			PropertyBag["Regions"] = regions;
			PropertyBag["RegionId"] = regions.Where(region => region.Name.ToLower().Equals("все")).First().Id;

			if (regioncode == null || from == null || to == null)
			{
				regioncode = Administrator.RegionMask;
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
		AND uui.UncommitedUpdateDate >= ?StartDateParam AND uui.UncommitedUpdateDate <= ?EndDateParam;
		 
SELECT cast(concat(count(if(resultid=2, PriceItemId, null)), '(', count(DISTINCT if(resultid=2, PriceItemId, null)), ')') as CHAR) as FormCount,
	   cast(ifnull(max(if(resultid=2, logtime, null)), '2000-01-01') as CHAR) as LastForm
FROM logs.formlogs
WHERE logtime >= ?StartDateParam AND logtime <= ?EndDateParam;
			 
SELECT cast(concat(count(if(resultcode=2, PriceItemId, null)), '(', count(DISTINCT if(resultcode=2, PriceItemId, null)), ')') as CHAR) as DownCount,
	   cast(ifnull(max(if(resultcode=2, logtime, null)), '2000-01-01') as CHAR) as LastDown
FROM logs.downlogs
WHERE logtime >= ?StartDateParam AND logtime <= ?EndDateParam;
			 
SELECT sum(ol.cost * ol.quantity) as OrderSum,
	   count(DISTINCT oh.rowid) as TotalOrders,
	   count(DISTINCT oh.clientcode) as UniqClientOrders,
	   count(DISTINCT oh.userid) as UniqUserOrders,
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
	  AND WriteTime >= ?StartDateParam AND WriteTime <= ?EndDateParam;

select count(oh.RowId) as NonProcOrdersCount
from orders.ordershead oh
where	oh.processed = 0
		and oh.submited = 1
		and oh.deleted = 0;

SELECT ifnull(sum(if(afu.UpdateType in (1,2), resultsize, 0)), 0) as DataSize,
	   ifnull(sum(if(afu.UpdateType in (8), resultsize, 0)), 0) as DocSize,
	   sum(if(afu.UpdateType = 6, 1, 0)) as UpdatesErr,
	   cast(concat(Sum(afu.UpdateType IN (5)) ,'(' ,count(DISTINCT if(afu.UpdateType  IN (5), u.Id, null)) ,')') as CHAR) UpdatesAD,
	   cast(concat(sum(afu.UpdateType = 2) ,'(' ,count(DISTINCT if(afu.UpdateType = 2, u.Id, null)) ,')') as CHAR) CumulativeUpdates,
	   cast(concat(sum(afu.UpdateType = 1) ,'(' ,count(DISTINCT if(afu.UpdateType = 1, u.Id, null)) ,')') as CHAR) Updates
FROM Future.Clients cd
	join Future.Users u on u.ClientId = cd.Id
	join logs.AnalitFUpdates afu on afu.UserId = u.Id
WHERE cd.maskregion & ?RegionMaskParam > 0
	  AND afu.RequestTime >= ?StartDateParam AND afu.RequestTime <= ?EndDateParam;

SELECT cast(concat(count(dlogs.RowId), '(', count(DISTINCT dlogs.ClientCode), ')') as CHAR) as CountDownloadedWaybills,
	   max(dlogs.LogTime) as LastDownloadedWaybillDate
FROM logs.document_logs dlogs
join usersettings.retclientsset rcs on rcs.ClientCode = dlogs.ClientCode
WHERE (dlogs.LogTime >= ?StartDateParam AND dlogs.LogTime <= ?EndDateParam) AND dlogs.DocumentType = 1 and rcs.ParseWaybills = 1;

SELECT cast(concat(count(dheaders.Id), '(', count(DISTINCT dheaders.ClientCode), ')') as CHAR) as CountParsedWaybills,
	   max(dheaders.WriteTime) as LastParsedWaybillDate
FROM documents.documentheaders dheaders
WHERE (dheaders.WriteTime >= ?StartDateParam AND dheaders.WriteTime <= ?EndDateParam) AND dheaders.DocumentType = 1;

select ifnull(sum(if(db.ProductId is not null, 1, 0)), 0) as DocumentProductIdentifiedCount,
	ifnull(sum(if(db.ProducerId is not null, 1, 0)), 0) as DocumentProducerIdentifiedCount,
	count(*) as DocumentLineCount
from documents.documentheaders d
	join documents.documentbodies db on db.DocumentId = d.Id
where (d.WriteTime >= ?StartDateParam AND d.WriteTime <= ?EndDateParam)", c);
					adapter.SelectCommand.Parameters.AddWithValue("?StartDateParam", fromDate);
					adapter.SelectCommand.Parameters.AddWithValue("?EndDateParam", toDate.AddDays(1));
					adapter.SelectCommand.Parameters.AddWithValue("?RegionMaskParam", regionMask & Administrator.RegionMask);
					adapter.Fill(data);
				});
			//Заказы
			//Количество принятых заказов
			PropertyBag["OPLB"] = String.Format("{0} ({1})",
									  data.Tables[3].Rows[0]["TotalOrders"],
									  data.Tables[3].Rows[0]["UniqUserOrders"]);
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
			// Количество загруженных накладных
			PropertyBag["CountDownloadedWaybills"] = data.Tables[6].Rows[0]["CountDownloadedWaybills"].ToString();
			// Дата последней загрузки накладной
			PropertyBag["LastDownloadedWaybillDate"] = data.Tables[6].Rows[0]["LastDownloadedWaybillDate"].ToString();
			// Количество разобранных накладных
			PropertyBag["CountParsedWaybills"] = data.Tables[7].Rows[0]["CountParsedWaybills"].ToString();
			// Дата последнего разбора накладной
			PropertyBag["LastParsedWaybillDate"] = data.Tables[7].Rows[0]["LastParsedWaybillDate"].ToString();

			PropertyBag["DocumentProductIdentifiedCount"] = data.Tables[8].Rows[0]["DocumentProductIdentifiedCount"];
			PropertyBag["DocumentProducerIdentifiedCount"] = data.Tables[8].Rows[0]["DocumentProducerIdentifiedCount"];
			PropertyBag["DocumentLineCount"] = data.Tables[8].Rows[0]["DocumentLineCount"];
		}

		[RequiredPermission(PermissionType.EditSettings)]
		public void Settings()
		{
			var defaults = DefaultValues.Get();
			if (IsPost)
			{
				BindObjectInstance(defaults, ParamStore.Form, "defaults", AutoLoadBehavior.Always);
				Flash["Message"] = Message.Notify("Сохранено");
				RedirectToReferrer();
			}
			else
			{
				PropertyBag["Defaults"] = defaults;
				PropertyBag["Formaters"] = OrderHandler.GetFormaters();
				PropertyBag["Senders"] = OrderHandler.GetSenders();
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
	}
}
