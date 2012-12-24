using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Suppliers;
using AdminInterface.Security;
using Common.Web.Ui.ActiveRecordExtentions;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using Common.Web.Ui.NHibernateExtentions;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;

namespace AdminInterface.Controllers.Filters
{
	public class DocumentFilter : PaginableSortable
	{
		public DocumentFilter()
		{
			Period = new DatePeriod(DateTime.Today.AddDays(-1), DateTime.Today);
			PageSize = 30;
		}
		[Description("Регион:")]
		public Region Region { get; set; }
		public DatePeriod Period { get; set; }
		[Description("Только неразобранные накладные: ")]
		public bool OnlyNoParsed { get; set; }

		private bool _statMode = false;

		public bool StatMode
		{
			get { return _statMode; }
			set { _statMode = value; }
		}

		public User User { get; set; }
		public Client Client { get; set; }
		public Supplier Supplier { get; set; }

		private IList<DocumentLog> AcceptPaginator(DetachedCriteria criteria)
		{
			var countQuery = CriteriaTransformer.TransformToRowCount(criteria);
			if(OnlyNoParsed) {
				if (CurrentPage > 0)
					criteria.SetFirstResult(CurrentPage * PageSize);

				criteria.SetMaxResults(PageSize);
			}

			return ArHelper.WithSession(s => {
				RowsCount = countQuery.GetExecutableCriteria(s).UniqueResult<int>();
				return criteria.GetExecutableCriteria(s).ToList<DocumentLog>();
			});
		}

		public IList<NotParcedStat> FindStat()
		{
			SortKeyMap = new Dictionary<string, string> {
				{ "Supplier", "Supplier" },
				{ "SupplierId", "SupplierId" },
				{ "RegionName", "RegionName" },
				{ "DocumentsCount", "DocumentsCount" }
			};
			OnlyNoParsed = true;
			var criteria = GetCriteriaForView(Period.Begin, Period.End);
			criteria.Add(Expression.IsNull("d.Id"));
			criteria.Add(Expression.Eq("DocumentType", DocumentType.Waybill));

			var documents = ArHelper.WithSession(s => criteria.GetExecutableCriteria(s).ToList<DocumentLog>());

			//var documents = AcceptPaginator(criteria);

			var groupedDocs = documents.GroupBy(d => d.SupplierId);
			var documentStats = new List<NotParcedStat>();
			foreach (var groupedDoc in groupedDocs) {
				var stat = new NotParcedStat {
					SupplierId = int.Parse(groupedDoc.Key),
					Supplier = groupedDoc.First().Supplier,
					RegionName = groupedDoc.First().RegionName,
					DocumentsCount = groupedDoc.Count()
				};
				foreach (var clientGroup in groupedDoc.GroupBy(s => s.ClientId)) {
					stat.DocumentToClientsCount += String.Format("{0}: {1}; ", clientGroup.First().Client, clientGroup.Count());
				}
				documentStats.Add(stat);
			}
			if(!String.IsNullOrEmpty(SortBy)) {
				var propertyInfo = typeof(NotParcedStat).GetProperty(SortBy);
				if(SortDirection.Contains("asc"))
					documentStats = documentStats.OrderBy(d => propertyInfo.GetValue(d, null)).ToList();
				else
					documentStats = documentStats.OrderByDescending(d => propertyInfo.GetValue(d, null)).ToList();
			}
			else
				documentStats = documentStats.OrderBy(d => d.Supplier).ThenBy(d => d.RegionName).ToList();
			return documentStats;
		}

		public IList<DocumentLog> Find()
		{
			var criteria = GetCriteriaForView(Period.Begin, Period.End);
			if (User != null)
				criteria.Add(Expression.Eq("sl.ForUser", User));

			if (Client != null)
				criteria.Add(Expression.Eq("ForClient", Client));

			if (Supplier != null)
				criteria.Add(Expression.Eq("FromSupplier", Supplier));

			if(OnlyNoParsed) {
				criteria.Add(Expression.IsNull("d.Id"));
				criteria.Add(Expression.Eq("DocumentType", DocumentType.Waybill));
			}

			return AcceptPaginator(criteria);
		}

		private DetachedCriteria GetCriteriaForView(DateTime begin, DateTime end)
		{
			var regionMask = SecurityContext.Administrator.RegionMask;
			if (Region != null)
				regionMask &= Region.Id;

			var criteria = DetachedCriteria.For<DocumentReceiveLog>();
			criteria.CreateAlias("ForClient", "fc", JoinType.LeftOuterJoin)
				.CreateAlias("Address", "a", JoinType.LeftOuterJoin)
				.CreateAlias("Document", "d", JoinType.LeftOuterJoin)
				.CreateAlias("SendUpdateLogEntity", "ru", JoinType.LeftOuterJoin);


			if(OnlyNoParsed)
				criteria.CreateCriteria("FromSupplier", "fs", JoinType.InnerJoin).Add(Expression.Sql("{alias}.HomeRegion & " + regionMask + " > 0"));
			else {
				criteria.CreateAlias("FromSupplier", "fs", JoinType.InnerJoin);
			}
			if((!OnlyNoParsed || User != null) && !StatMode) {
				criteria.CreateAlias("SendLogs", "sl", JoinType.LeftOuterJoin);
				criteria.CreateAlias("sl.ForUser", "u", JoinType.LeftOuterJoin);
				criteria.CreateAlias("sl.SendedInUpdate", "su", JoinType.LeftOuterJoin);
			}
			var projection = Projections.ProjectionList();
			projection.Add(Projections.Property<DocumentReceiveLog>(d => d.Id).As("Id"))
				.Add(Projections.Property<DocumentReceiveLog>(d => d.LogTime).As("LogTime"))
				.Add(Projections.Property<DocumentReceiveLog>(d => d.DocumentType).As("DocumentType"))
				.Add(Projections.Property<DocumentReceiveLog>(d => d.FileName).As("FileName"))
				.Add(Projections.Property<DocumentReceiveLog>(d => d.Addition).As("Addition"))
				.Add(Projections.Property<DocumentReceiveLog>(d => d.DocumentSize).As("DocumentSize"))
				.Add(Projections.Property("ru.Id").As("SendUpdateId"))
				.Add(Projections.Property("d.Id").As("DocumentId"))
				.Add(Projections.Property("d.ProviderDocumentId").As("ProviderDocumentId"))
				.Add(Projections.Property("d.DocumentDate").As("DocumentDate"))
				.Add(Projections.Property("d.WriteTime").As("DocumentWriteTime"))
				.Add(Projections.Property("fs.Name").As("Supplier"))
				.Add(Projections.Property("fs.Id").As("SupplierId"))
				.Add(Projections.Property("fc.Name").As("Client"))
				.Add(Projections.Property("fc.Id").As("ClientId"))
				.Add(Projections.Property("a.Value").As("Address"))
				.Add(Projections.Property("fs.HomeRegion").As("RegionName"));
			if(!OnlyNoParsed && !StatMode) {
				projection.Add(Projections.Property("u.Login").As("Login"))
					.Add(Projections.Property("u.Id").As("LoginId"));
				projection.Add(Projections.Property("su.RequestTime").As("RequestTime"))
					.Add(Projections.Property("sl.Id").As("DeliveredId"))
					.Add(Projections.Property("sl.FileDelivered").As("FileDelivered"))
					.Add(Projections.Property("sl.DocumentDelivered").As("DocumentDelivered"));
			}
			criteria.SetProjection(projection)
				.Add(Expression.Ge("LogTime", begin))
				.Add(Expression.Le("LogTime", end.AddDays(1)))
				.AddOrder(Order.Desc("LogTime"));

			if(OnlyNoParsed) {
				criteria.Add(Expression.Eq("IsFake", false));
				criteria.Add(Expression.Or(Expression.IsNull(Projections.Property<DocumentReceiveLog>(d => d.Addition)),
					Expression.Not(Expression.Eq(Projections.Property<DocumentReceiveLog>(d => d.Addition), "Сконвертированный файл"))));
			}
			return criteria;
		}
	}

	public class DocumentLog
	{
		public uint Id { get; set; }
		public DateTime LogTime { get; set; }
		public DocumentType DocumentType { get; set; }
		public string Addition { get; set; }
		public string FileName { get; set; }
		public uint? DocumentSize { get; set; }

		public uint? SendUpdateId { get; set; }

		public uint? DocumentId { get; set; }
		public string ProviderDocumentId { get; set; }
		public DateTime? DocumentDate { get; set; }
		public DateTime? DocumentWriteTime { get; set; }

		public string Supplier { get; set; }
		public string SupplierId { get; set; }
		public string Client { get; set; }
		public string ClientId { get; set; }
		public string Address { get; set; }
		public string Login { get; set; }
		public string LoginId { get; set; }
		public DateTime? RequestTime { get; set; }
		public uint DeliveredId { get; set; }
		public bool? FileDelivered { get; set; }
		public bool? DocumentDelivered { get; set; }
		public string RegionName { get; set; }

		public bool DocumentProcessedSuccessfully()
		{
			return (DeliveredId <= 15374942) || (FileDelivered.HasValue && FileDelivered.Value) || (DocumentDelivered.HasValue && DocumentDelivered.Value);
		}

		public DateTime? GetDisplayRequestTime()
		{
			if (DocumentProcessedSuccessfully())
				return RequestTime;

			return null;
		}
	}

	public class NotParcedStat
	{
		[Display(Name = "Наименование поставщика", Order = 1)]
		public string Supplier { get; set; }
		[Display(Name = "Код поставщика", Order = 0)]
		public int SupplierId { get; set; }
		[Display(Name = "Регион", Order = 2)]
		public string RegionName { get; set; }
		[Display(Name = "Кол-во нераспознанных накладных", Order = 3)]
		public int DocumentsCount { get; set; }
		[Display(Name = "Кол-во нераспознанных накладных отправленных клиентам", Order = 4)]
		public string DocumentToClientsCount { get; set; }
	}
}