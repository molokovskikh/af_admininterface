using System;
using System.ComponentModel;
using System.Data;
using AdminInterface.Security;
using Common.MySql;
using Common.Tools;
using MySql.Data.MySqlClient;

namespace AdminInterface.Controllers.Filters
{
	public class StatQuery
	{
		[Description("Полностью")]
		public bool Full { get; set; }

		public DataSet Load(ulong regionMask, DateTime fromDate, DateTime toDate)
		{
			var data = new DataSet();

			var additionalSql =@"
SELECT count(dlogs.RowId) as CountDownloadedWaybills,
	max(dlogs.LogTime) as LastDownloadedWaybillDate,
	count(distinct dlogs.ClientCode) as CountDownloadedWaybilsByClient,
	count(distinct dlogs.FirmCode) as CountDownloadedWaybilsBySupplier
FROM logs.document_logs dlogs
join usersettings.retclientsset rcs on rcs.ClientCode = dlogs.ClientCode
WHERE (dlogs.LogTime >= ?StartDateParam AND dlogs.LogTime <= ?EndDateParam) AND dlogs.DocumentType = 1 and rcs.ParseWaybills = 1;

SELECT count(distinct dsl.UserId) as CountDownloadedWaybilsByUser
FROM logs.document_logs dl
		join usersettings.retclientsset rcs on rcs.ClientCode = dl.ClientCode
	join Logs.DocumentSendLogs dsl on dsl.DocumentId = dl.RowId
WHERE (dl.LogTime >= ?StartDateParam AND dl.LogTime <= ?EndDateParam) AND dl.DocumentType = 1 and rcs.ParseWaybills = 1;

SELECT count(dheaders.Id) as CountParsedWaybills,
	max(dheaders.WriteTime) as LastParsedWaybillDate,
	count(distinct dheaders.ClientCode) as CountParsedWaybillsByClient,
	count(distinct dheaders.FirmCode) as CountParsedWaybillsBySupplier
FROM documents.documentheaders dheaders
WHERE (dheaders.WriteTime >= ?StartDateParam AND dheaders.WriteTime <= ?EndDateParam) AND dheaders.DocumentType = 1;

SELECT count(distinct dsl.UserId) as CountParsedWaybillsByUser
FROM documents.documentheaders dh
	join Logs.Document_Logs dl on dl.RowId = dh.DownloadId
		join Logs.DocumentSendLogs dsl on dsl.DocumentId = dl.RowId
WHERE (dh.WriteTime >= ?StartDateParam AND dh.WriteTime <= ?EndDateParam) and dh.DocumentType = 1;

select ifnull(sum(if(db.ProductId is not null, 1, 0)), 0) as DocumentProductIdentifiedCount,
	ifnull(sum(if(db.ProducerId is not null, 1, 0)), 0) as DocumentProducerIdentifiedCount,
	count(*) as DocumentLineCount
from documents.documentheaders d
	join documents.documentbodies db on db.DocumentId = d.Id
where (d.WriteTime >= ?StartDateParam AND d.WriteTime <= ?EndDateParam);

select count(*) as TotalCertificates,
sum(if(l.Filename is null, 1, 0)) as TotalNotSendCertificates,
sum(if(l.Filename is not null, 1, 0)) as TotalSendCertificates,

count(distinct fu.Id) as CertificateUniqUsers,
count(distinct c.Id) as CertificateUniqClients,
count(distinct dh.FirmCode) as CertificateUniqSuppliers,

count(distinct if(l.Filename is null, fu.Id, null)) as CertificateSendUniqUsers,
count(distinct if(l.Filename is null, c.Id, null)) as CertificateSendUniqClients,
count(distinct if(l.Filename is null, dh.FirmCode, null)) as CertificateSendUniqSuppliers,

count(distinct if(l.Filename is not null, fu.Id, null)) as CertificateNotSendUniqUsers,
count(distinct if(l.Filename is not null, c.Id, null)) as CertificateNotSendUniqClients,
count(distinct if(l.Filename is not null, dh.FirmCode, null)) as CertificateNotSendUniqSuppliers

from Logs.CertificateRequestLogs l
	join Logs.AnalitFUpdates u on u.UpdateId = l.UpdateId
		join Future.Users fu on fu.Id = u.UserId
			join Future.Clients c on c.Id = fu.ClientId
	join Documents.DocumentBodies db on db.Id = l.DocumentBodyId
		join Documents.DocumentHeaders dh on dh.Id = db.DocumentId
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

select count(distinct u.Id) as MailsUniqByUser,
	count(distinct c.Id) as  MailsUniqByClient,
	count(distinct m.SupplierId) as MailsUniqBySupplier
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
;";
				var mainSql = @"
SELECT max(UpdateDate) MaxUpdateTime
FROM future.Clients cd
	join future.Users u on u.ClientId = cd.Id
		JOIN usersettings.UserUpdateInfo uui on uui.UserId = u.Id
WHERE   cd.RegionCode & ?RegionMaskParam > 0
		AND uui.UncommitedUpdateDate >= ?StartDateParam AND uui.UncommitedUpdateDate <= ?EndDateParam;

SELECT cast(concat(count(if(resultid=2, PriceItemId, null)), '(', count(DISTINCT if(resultid=2, PriceItemId, null)), ')') as CHAR) as FormCount,
		max(if(resultid=2, logtime, null)) as LastForm
FROM logs.formlogs
WHERE logtime >= ?StartDateParam AND logtime <= ?EndDateParam;

SELECT cast(concat(count(if(resultcode=2, PriceItemId, null)), '(', count(DISTINCT if(resultcode=2, PriceItemId, null)), ')') as CHAR) as DownCount,
		max(if(resultcode=2, logtime, null)) as LastDown
FROM logs.downlogs
WHERE logtime >= ?StartDateParam AND logtime <= ?EndDateParam;

SELECT count(DISTINCT oh.clientcode) as UniqClientOrders,
	   max(WriteTime) as MaxOrderTime
		{1}
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

SELECT sum(if(afu.UpdateType = 6, 1, 0)) as UpdatesErr,
	   cast(concat(Sum(afu.UpdateType IN (5)) ,'(' ,count(DISTINCT if(afu.UpdateType  IN (5), u.Id, null)) ,')') as CHAR) UpdatesAD,
	   cast(concat(sum(afu.UpdateType = 2) ,'(' ,count(DISTINCT if(afu.UpdateType = 2, u.Id, null)) ,')') as CHAR) CumulativeUpdates,
	   cast(concat(sum(afu.UpdateType = 1) ,'(' ,count(DISTINCT if(afu.UpdateType = 1, u.Id, null)) ,')') as CHAR) Updates
		{0}
FROM Future.Clients cd
	join Future.Users u on u.ClientId = cd.Id
	join logs.AnalitFUpdates afu on afu.UserId = u.Id
WHERE cd.maskregion & ?RegionMaskParam > 0
	  AND afu.RequestTime >= ?StartDateParam AND afu.RequestTime <= ?EndDateParam;


";
			var additionalUpdateStatColumns = new[] {
				"ifnull(sum(if(afu.UpdateType in (1,2), resultsize, 0)), 0) as DownloadDataSize",
				"ifnull(sum(if(afu.UpdateType in (8), resultsize, 0)), 0) as DownloadDocumentSize"
			};

			var additionalOrderColumns = new[] {
				"count(DISTINCT oh.UserId) as UniqUserOrders",
				"count(DISTINCT oh.rowid) as TotalOrders",
				"count(DISTINCT oh.ClientCode) as UniqClientOrders",
				"sum(ol.cost * ol.quantity) as OrderSum"
			};
			string sql;

			if (Full)
			{
				sql = String.Format(mainSql,
					"," + additionalUpdateStatColumns.Implode(","),
					"," + additionalOrderColumns.Implode(","));
			}
			else
			{
				sql = String.Format(mainSql, "", "");
			}

			if (Full)
				sql += additionalSql;

			With.Connection(c => {
				var adapter = new MySqlDataAdapter(sql, c);
				adapter.SelectCommand.Parameters.AddWithValue("?StartDateParam", fromDate);
				adapter.SelectCommand.Parameters.AddWithValue("?EndDateParam", toDate.AddDays(1));
				adapter.SelectCommand.Parameters.AddWithValue("?RegionMaskParam", regionMask & SecurityContext.Administrator.RegionMask);
				adapter.Fill(data);
			});
			return data;
		}
	}
}