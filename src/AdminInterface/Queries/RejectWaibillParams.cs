using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Common.Web.Ui.NHibernateExtentions;
using NHibernate;

namespace AdminInterface.Queries
{
	public class RejectWaibillParams
	{
		public bool SendWaybills { get; set;}
		public bool SendRejects { get; set;}

		public RejectWaibillParams Get(uint clientId, ISession DbSession)
		{
			var rejectWaibillParams = DbSession.CreateSQLQuery(@"
select SendRejects, SendWaybills from
(select count(*) as cou, u.Id, u.SendRejects, SendWaybills from Customers.Users u
where clientId = :client
group by u.SendWaybills and u.SendRejects) as k
order by cou desc;")
			.SetParameter("client", clientId)
			.ToList<RejectWaibillParams>().FirstOrDefault();
			if (rejectWaibillParams != null) {
				SendRejects = rejectWaibillParams.SendRejects;
				SendWaybills = rejectWaibillParams.SendWaybills;
			}
			return this;
		}
	}
}