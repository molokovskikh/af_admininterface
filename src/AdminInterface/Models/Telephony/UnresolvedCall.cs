using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Linq;

namespace AdminInterface.Models.Telephony
{
	[ActiveRecord("telephony.UnresolvedPhone")]
	public class UnresolvedCall : ActiveRecordLinqBase<UnresolvedCall>
	{
		[PrimaryKey("id")]
		public virtual ulong Id { get; set; }

		[Property("Phone")]
		public virtual string PhoneNumber { get; set; }

		public static string[] LastCalls
		{
			get
			{
				return (from call in Queryable
						orderby call.Id descending
						group call by call.PhoneNumber into c
						select c.Key).Take(5).ToArray();
			}
		}
	}
}
