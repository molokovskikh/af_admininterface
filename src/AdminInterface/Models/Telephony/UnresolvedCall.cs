using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.ActiveRecord.Linq;

namespace AdminInterface.Models.Telephony
{
	[ActiveRecord("UnresolvedPhone", Schema = "telephony")]
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
				return new string[0];
/*				return (from call in Queryable
						orderby call.Id descending
						group call by call.PhoneNumber).Take(5).ToArray().Select(c => c.Key).ToArray();*/
/*
//by call.PhoneNumber into c
						select c.Key).Take(5).ToArray();
*/
			}
		}
	}
}
