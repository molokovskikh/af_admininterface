using System;
using System.Collections.Generic;
using AdminInterface.Security;
using Common.Web.Ui.Helpers;
using NHibernate.Transform;

namespace AdminInterface.Models.Logs
{
	public class Updates : ClientRegistrationLogEntity
	{
		public DateTime? LastUpdate { get; set; }
		public DateTime? LastUncommitedUpdate { get; set; }

		public DateTime? GetUncomitedUpdate()
		{
			if (LastUncommitedUpdate == null)
				return null;

			if (LastUpdate == null)
				return LastUncommitedUpdate;

			if (LastUncommitedUpdate > LastUpdate)
				return LastUncommitedUpdate;

			return null;
		}
	}

	public class Orders : ClientRegistrationLogEntity
	{
		public long SuppliersCount { get; set; }
		public decimal OrdersSum { get; set; }
		public long OrdersCount { get; set; }
	}

	public class ClientRegistrationLogEntity
	{
		public uint ParentId { get; set; }

		public string Parent { get; set; }

		public string IncludeType { get; set; }

		public uint BillingCode { get; set; }

		public uint FirmCode { get; set; } 

		public string ShortName { get; set; }

		public string Region { get; set; }

		public DateTime RegistrationDate { get; set; }

		public string Registrant { get; set; }

		public string ManagerName { get; set; }

		public string GetInclude()
		{
			if (String.IsNullOrEmpty(IncludeType))
				return "";
			var value = Convert.ToUInt32(IncludeType);
			var type = (RelationshipType) value;
			return BindingHelper.GetDescription(type);
		}

		public bool HaveParent()
		{
			return ParentId != 0;
		}

		public string GetRegistrant()
		{
			if (String.IsNullOrEmpty(ManagerName))
				return Registrant;
			return ManagerName;
		}

		public static IList<ClientRegistrationLogEntity> NotUpdated(uint days, uint filter, ulong regionCode, string sortby, string direction)
		{
			return ArHelper.WithSession(session => {
				if (regionCode == 0)
					regionCode = ulong.MaxValue;

				if (direction == "descending")
					direction = "desc";
				else
					direction = "asc";

				if (String.IsNullOrEmpty(sortby))
					sortby = "ShortName";

				var clientTypeFilter = "";
				if (filter == 1)
					clientTypeFilter = "and (rcs.InvisibleOnFirm <> 2 and ifnull(ir.IncludeType, 555) <> 2)";
				else if (filter == 2)
					clientTypeFilter = "and (rcs.InvisibleOnFirm = 2 or ifnull(ir.IncludeType, 555) = 2)";

				var items = session.CreateSQLQuery(String.Format(@"
select	cd.BillingCode,
		cd.FirmCode,
		cd.ShortName,
		r.Region,
		cd.RegistrationDate,
		cd.Registrant,
		ra.ManagerName,
		ir.PrimaryClientCode as ParentId,
		parent.ShortName as Parent,
		cast(ir.IncludeType as CHAR) as IncludeType,

		max(if(au.Commit = 1, au.RequestTime, null)) LastUpdate,
		max(if(au.Commit = 0, au.RequestTime, null)) LastUncommitedUpdate
from clientsdata cd
	join farm.Regions r on r.RegionCode = cd.RegionCode
	join usersettings.RetClientsSet rcs on rcs.ClientCode = cd.FirmCode
	left join includeregulation ir ON ir.includeclientcode = cd.firmcode
		left join clientsdata parent on parent.FirmCode = ir.PrimaryClientCode
	join usersettings.OsUserAccessRight ouar on ouar.clientcode = if(ir.PrimaryClientCode is not null and ir.IncludeType = 0, ir.PrimaryClientCode, cd.FirmCode)
		left join logs.AnalitFUpdates au on au.UserId = ouar.RowId and (au.updatetype = 1 or au.updatetype = 2 or au.updatetype is null)
	left join accessright.regionaladmins ra on ra.UserName = cd.Registrant
where	cd.firmtype = 1
		and cd.firmstatus = 1
		and cd.billingstatus = 1
		and cd.firmsegment = 0
		and cd.RegionCode & :adminRegionMask > 0

		and (ir.PrimaryClientCode is null or ir.IncludeType <> 0)
		{2}
group by cd.firmcode
having LastUpdate < (now() - interval :days day)
order by {0} {1}", sortby, direction, clientTypeFilter))
					.SetParameter("days", days)
					.SetParameter("adminRegionMask", SecurityContext.Administrator.RegionMask & regionCode)
					.SetResultTransformer(Transformers.AliasToBean<Updates>())
					.List<ClientRegistrationLogEntity>();

				return items;
			});
		}


		public static IList<ClientRegistrationLogEntity> NotOrdered(uint days, uint filter, ulong regionCode, string sortby, string direction)
		{
			return ArHelper.WithSession(session => {

				if (regionCode == 0)
					regionCode = ulong.MaxValue;

				if (direction == "descending")
					direction = "desc";
				else
					direction = "asc";

				if (String.IsNullOrEmpty(sortby))
					sortby = "ShortName";

				var clientTypeFilter = "";
				if (filter == 1)
					clientTypeFilter = "and rcs.InvisibleOnFirm <> 2";
				else if (filter == 2)
					clientTypeFilter = "and rcs.InvisibleOnFirm = 2";

				return session.CreateSQLQuery(String.Format(@"
select	cd.BillingCode,
		cd.FirmCode,
		cd.ShortName,
		r.Region,
		cd.RegistrationDate,
		cd.Registrant,
		ra.ManagerName,
		ir.PrimaryClientCode as ParentId,
		parent.ShortName as Parent,
		cast(ir.IncludeType as CHAR) as IncludeType,

		count(distinct pd.FirmCode) SuppliersCount,
		sum((select sum(ol.Quantity * ol.Cost) from orders.orderslist ol where ol.orderid = oh.rowid)) OrdersSum,
		count(oh.RowId) OrdersCount
from clientsdata cd
	join farm.Regions r on r.RegionCode = cd.RegionCode
	join usersettings.RetClientsSet rcs on rcs.ClientCode = cd.FirmCode
	left join includeregulation ir ON ir.includeclientcode = cd.firmcode
		left join clientsdata parent on parent.FirmCode = ir.PrimaryClientCode
	left join orders.OrdersHead oh on oh.ClientCode = cd.FirmCode and DATEDIFF(now(), oh.WriteTime) < :days and oh.Deleted = 0 and oh.Submited = 1
		left join usersettings.pricesdata pd on pd.PriceCode = oh.PriceCode
	left join accessright.regionaladmins ra on ra.UserName = cd.Registrant
where	cd.firmtype = 1
		and cd.firmstatus = 1
		and cd.billingstatus = 1
		and cd.firmsegment = 0
		and cd.RegionCode & :adminRegionMask > 0

		{2}
group by cd.firmcode
having SuppliersCount < 4 or OrdersSum < :days * 1000 or OrdersCount < :days * 1
order by {0} {1}", sortby, direction, clientTypeFilter))
							.SetParameter("days", days)
							.SetParameter("adminRegionMask", SecurityContext.Administrator.RegionMask & regionCode)
							.SetResultTransformer(Transformers.AliasToBean<Orders>())
							.List<ClientRegistrationLogEntity>();
				});
		}
	}
}
