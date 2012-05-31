using System;
using System.Collections.Generic;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Suppliers;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.NHibernateExtentions;
using NHibernate.Criterion;
using NHibernate.SqlCommand;

namespace AdminInterface.Controllers.Filters
{
	public class DocumentFilter
	{
		public DocumentFilter()
		{
			Period = new DatePeriod(DateTime.Today.AddDays(-1), DateTime.Today);
		}

		public DatePeriod Period { get; set; }

		public User User { get; set; }
		public Client Client { get; set; }
		public Supplier Supplier { get; set; }

		public IList<DocumentLog> Find()
		{
			var criteria = GetCriteriaForView(Period.Begin, Period.End);
			if (User != null)
				criteria.Add(Expression.Eq("sl.ForUser", User));

			if (Client != null)
				criteria.Add(Expression.Eq("ForClient", Client));

			if (Supplier != null)
				criteria.Add(Expression.Eq("FromSupplier", Supplier));

			return ArHelper.WithSession(
				s => criteria
					.GetExecutableCriteria(s)
					.ToList<DocumentLog>());
		}

		private static DetachedCriteria GetCriteriaForView(DateTime begin, DateTime end)
		{
			return DetachedCriteria.For<DocumentReceiveLog>()
				.CreateAlias("FromSupplier", "fs", JoinType.InnerJoin)
				.CreateAlias("ForClient", "fc", JoinType.LeftOuterJoin)
				.CreateAlias("Address", "a", JoinType.LeftOuterJoin)
				.CreateAlias("Document", "d", JoinType.LeftOuterJoin)
				.CreateAlias("SendLogs", "sl", JoinType.LeftOuterJoin)
				.CreateAlias("SendUpdateLogEntity", "ru", JoinType.LeftOuterJoin)
				.CreateAlias("sl.ForUser", "u", JoinType.LeftOuterJoin)
				.CreateAlias("sl.SendedInUpdate", "su", JoinType.LeftOuterJoin)
				.SetProjection(
					Projections.ProjectionList()
						.Add(Projections.Property<DocumentReceiveLog>(d => d.Id).As("Id"))
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
						.Add(Projections.Property("u.Login").As("Login"))

						.Add(Projections.Property("u.Id").As("LoginId"))

						.Add(Projections.Property("su.RequestTime").As("RequestTime"))
				)
				.Add(Expression.Ge("LogTime", begin))
				.Add(Expression.Le("LogTime", end.AddDays(1)))
				.AddOrder(Order.Desc("LogTime"));
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
	}
}