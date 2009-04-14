using System;
using System.Collections.Generic;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Common.Web.Ui.Helpers;

namespace AdminInterface.Models.Logs
{
	[ActiveRecord]
	public class ClientRegistrationLogEntity
	{
		[PrimaryKey("ClientCode")]
		public uint ClientCode { get; set; } 

		[Property]
		public string ClientName { get; set; }

		[Property]
		public DateTime RegistrationDate { get; set; }

		[Property]
		public string RegistredBy { get; set; }

		[Property]
		public DateTime? LastUpdateDate { get; set; }

		[Property]
		public DateTime? LastUncommitedUpdate{ get; set;}

		public static IList<ClientRegistrationLogEntity> GetEntitiesForPeriond(DateTime beginDate, DateTime endDate, int dayWithoutUpdate)
		{
			return ArHelper.WithSession(
				session => session.CreateSQLQuery(String.Format(@"
select	cd.FirmCode as {{ClientRegistrationLogEntity.ClientCode}},
		cd.ShortName as {{ClientRegistrationLogEntity.ClientName}}, 
		cd.RegistrationDate as {{ClientRegistrationLogEntity.RegistrationDate}}, 
		cd.Registrant as {{ClientRegistrationLogEntity.RegistredBy}}, 
		cast(max(if(au.Commit = 1, au.RequestTime, null)) as CHAR) as {{ClientRegistrationLogEntity.LastUpdateDate}},
		cast(max(if(au.Commit = 0, au.RequestTime, null)) as CHAR) as {{ClientRegistrationLogEntity.LastUncommitedUpdate}}
from clientsdata cd
  left join logs.AnalitFUpdates au on au.clientcode = cd.firmcode
where firmtype = 1
      and firmstatus = 1
      and billingstatus = 1
      and registrationdate between :beginDate and :endDate
      and firmsegment = 0
      and (updatetype = 1 or updatetype = 2 or updatetype is null)
	  and cd.RegionCode & :adminRegionMask > 0
	  {0}
group by cd.firmcode
having DATEDIFF(now(), {{ClientRegistrationLogEntity.LastUpdateDate}}) > :dayWithoutUpdate or {{ClientRegistrationLogEntity.LastUpdateDate}} is null
order by cd.RegistrationDate", SecurityContext.Administrator.GetClientFilterByType("cd")))
							.AddEntity(typeof(ClientRegistrationLogEntity))
							.SetParameter("beginDate", beginDate)
							.SetParameter("endDate", endDate)
							.SetParameter("dayWithoutUpdate", dayWithoutUpdate)
							.SetParameter("adminRegionMask", SecurityContext.Administrator.RegionMask)
							.List<ClientRegistrationLogEntity>());
		}

		public bool IsUpdateToOld()
		{
			if (LastUpdateDate == null)
				return true;
			return (DateTime.Today - LastUpdateDate.Value).Days > 2;
		}

		public bool HaveOnlyNotCommitedUpdates()
		{
			if (LastUncommitedUpdate != null && LastUpdateDate == null)
				return true;
			return false;
		}
	}
}
