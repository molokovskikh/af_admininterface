using System;
using System.Collections.Generic;
using System.Linq;
using Castle.ActiveRecord;

namespace AdminInterface.Models.Logs
{
	[ActiveRecord("AddressLogs", Schema = "logs")]
	public class AddressLogRecord : ActiveRecordBase<AddressLogRecord>
	{
		[PrimaryKey]
		public virtual uint Id { get; set; }

		[Property]
		public virtual DateTime LogTime { get; set; }

		[Property]
		public virtual string OperatorName { get; set; }

		[BelongsTo("AddressId")]
		public virtual Address Address { get; set; }

		[Property]
		public virtual LogOperation Operation { get; set; }

		[Property]
		public virtual bool Enabled { get; set; }

		[Property]
		public virtual string Comment { get; set; }

		public static IList<AddressLogRecord> GetLogs(IEnumerable<Address> addresses)
		{
			if (addresses.Count() == 0)
				return Enumerable.Empty<AddressLogRecord>().ToList();

			return (List<AddressLogRecord>)Execute(
				(session, instance) =>
					session.CreateSQLQuery(@"
select {AddressLogRecord.*}
from logs.AddressLogs {AddressLogRecord}
where {AddressLogRecord}.Enabled is not null
		and {AddressLogRecord}.AddressId in (:addressIds)
order by logtime desc
limit 100")
				.AddEntity(typeof(AddressLogRecord))
				.SetParameterList("addressIds", addresses.Select(a => a.Id).ToList())
				.List<AddressLogRecord>(), null);
		}

		public static AddressLogRecord LastOff(uint addressId)
		{
			return (AddressLogRecord)Execute(
				(session, instance) =>
					session.CreateSQLQuery(@"
select {AddressLogRecord.*}
from logs.AddressLogs {AddressLogRecord}
where Enabled = 0
		and AddressId = :AddressId
order by logtime desc
limit 1")
				.AddEntity(typeof(AddressLogRecord))
				.SetParameter("AddressId", addressId)
				.UniqueResult(), null);
		}
	}
}
