using System;
using System.Collections.Generic;
using System.Linq;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;

namespace AdminInterface.Models.Logs
{
	[ActiveRecord(Schema = "logs")]
	public class SupplierLog : ActiveRecordLinqBase<SupplierLog>
	{
		[PrimaryKey]
		public virtual uint Id { get; set; }

		[Property]
		public virtual DateTime LogTime { get; set; }

		[Property]
		public virtual string OperatorName { get; set; }

		[BelongsTo("SupplierId")]
		public virtual Supplier Supplier { get; set; }

		[Property]
		public virtual bool? Disabled { get; set; }

		[Property]
		public virtual string Comment { get; set; }

		public static IList<SupplierLog> GetLogs(IEnumerable<Supplier> suppliers)
		{
			if (suppliers.Count() == 0)
				return Enumerable.Empty<SupplierLog>().ToList();

			return (List<SupplierLog>)Execute(
				(session, instance) => session.CreateSQLQuery(@"
select {SupplierLog.*}
from logs.SupplierLogs {SupplierLog}
where disabled is not null
		and supplierId in (:supplierId)
order by logtime desc
limit 100")
					.AddEntity(typeof(SupplierLog))
					.SetParameterList("supplierId", suppliers.Select(c => c.Id).ToArray())
					.List<SupplierLog>(), null);
		}
	}
}