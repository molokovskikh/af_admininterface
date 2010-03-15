using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Castle.ActiveRecord;

namespace AdminInterface.Models.Logs
{
	[ActiveRecord("logs.AddressLogs")]
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
		public virtual bool Enabled { get; set; }

		public static IList<AddressLogRecord> GetAddressLogRecords(Address address)
		{
			return (List<AddressLogRecord>)Execute(
				(session, instance) =>
					session.CreateSQLQuery(@"
select {AddressLogRecord.*}
from logs.AddressLogs {AddressLogRecord}
where {AddressLogRecord}.Enabled is not null
		and {AddressLogRecord}.AddressId = :AddressId
order by logtime desc
limit 5")
				.AddEntity(typeof(AddressLogRecord))
				.SetParameter("AddressId", address.Id)
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
