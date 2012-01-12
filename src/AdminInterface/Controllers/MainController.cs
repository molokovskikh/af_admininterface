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
		Helper(typeof (BindingHelper)),
		Secure
	]
	public class MainController : AdminInterfaceController
	{
		public MainController()
		{
			SetBinder(new ARDataBinder());
		}

		public void Index(ulong? regioncode, DateTime? from, DateTime? to)
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
			GetStatistics(regioncode.Value, from.Value, to.Value);
		}

		private void GetStatistics(ulong regionMask, DateTime fromDate, DateTime toDate)
		{
			PropertyBag["RegionMask"] = regionMask;
			PropertyBag["FromDate"] = fromDate;
			PropertyBag["ToDate"] = toDate;

			var data = new DataSet();
			With.Connection(c => {
				var adapter =
					new MySqlDataAdapter(
						@"
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
FROM (orders.ordershead oh,
	 orders.orderslist ol, 
	 future.Clients cd, 
	 usersettings.retclientsset rcs)
	join Future.Users u on u.Id = oh.UserId
WHERE oh.rowid = orderid
	AND cd.Id = oh.clientcode
	AND u.PayerId <> 921
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

SELECT ifnull(sum(if(afu.UpdateType in (1,2), resultsize, 0)), 0) as DownloadDataSize,
	   ifnull(sum(if(afu.UpdateType in (8), resultsize, 0)), 0) as DownloadDocumentSize,
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
where (d.WriteTime >= ?StartDateParam AND d.WriteTime <= ?EndDateParam);

select count(*) as TotalCertificates,
sum(if(l.Filename is null, 1, 0)) as TotalNotSendCertificates,
sum(if(l.Filename is not null, 1, 0)) as TotalSendCertificates
from Logs.CertificateRequestLogs l
	join Logs.AnalitFUpdates u on u.UpdateId = l.UpdateId
		join Future.Users fu on fu.Id = u.UserId
			join Future.Clients c on c.Id = fu.ClientId
where (u.RequestTime >= ?StartDateParam AND u.RequestTime <= ?EndDateParam)
and c.MaskRegion & ?RegionMaskParam > 0
;

select max(u.RequestTime) as LastCertificateRequest
from Logs.CertificateRequestLogs l
	join Logs.AnalitFUpdates u on u.UpdateId = l.UpdateId
		join Future.Users fu on fu.Id = u.UserId
			join Future.Clients c on c.Id = fu.ClientId
where c.MaskRegion & ?RegionMaskParam > 0
;

select count(*) TotalMails,
max(m1.Size) MaxMailSize,
avg(m1.Size) AvgMailSize
from (
		select m.Id
		from Documents.Mails m
			join Logs.MailSendLogs l on l.MailId = m.Id
				join Future.Users u on u.Id = l.UserId
					join Future.Clients c on c.Id = u.ClientId
		where m.LogTime >= ?StartDateParam AND m.LogTime <= ?EndDateParam
			and c.MaskRegion & ?RegionMaskParam > 0
		group by m.Id
	) as bm
	join Documents.Mails m1 on m1.Id = bm.Id
;

select count(*) as TotalMailRecipients
from Documents.Mails m
	join Logs.MailSendLogs l on l.MailId = m.Id
		join Future.Users u on u.Id = l.UserId
			join Future.Clients c on c.Id = u.ClientId
where m.LogTime >= ?StartDateParam AND m.LogTime <= ?EndDateParam
	and c.MaskRegion & ?RegionMaskParam > 0
;

select max(m.LogTime) as LastMailSend
from Documents.Mails m
	join Logs.MailSendLogs l on l.MailId = m.Id
		join Future.Users u on u.Id = l.UserId
			join Future.Clients c on c.Id = u.ClientId
where c.MaskRegion & ?RegionMaskParam > 0
;
",
						c);
				adapter.SelectCommand.Parameters.AddWithValue("?StartDateParam", fromDate);
				adapter.SelectCommand.Parameters.AddWithValue("?EndDateParam", toDate.AddDays(1));
				adapter.SelectCommand.Parameters.AddWithValue("?RegionMaskParam", regionMask & Admin.RegionMask);
				adapter.Fill(data);
			});
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
					PropertyBag[column.ColumnName] = value;
				}
			}

			Size("MaxMailSize");
			Size("AvgMailSize");
			Size("DownloadDataSize");
			Size("DownloadDocumentSize");
			InPercentOf("TotalNotSendCertificates", "TotalCertificates");
			InPercentOf("TotalSendCertificates", "TotalCertificates");
			PropertyBag["OrderSum"] = Convert.ToDouble(data.Tables[3].Rows[0]["OrderSum"].IfDbNull(0)).ToString("C");
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
				Notify("Сохранено");
				RedirectToReferrer();
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
	}
}
