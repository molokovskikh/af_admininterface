using System;
using System.Collections.Generic;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Common.Web.Ui.Helpers;
using Common.MySql;

namespace AdminInterface.Models
{
	[ActiveRecord(SchemaAction = "none")]
	public class BillingSearchItem : ActiveRecordBase
	{
		[PrimaryKey]
		public uint BillingCode { get; set; }

		[Property]
		public string ShortName { get; set; }

		[Property]
		public string Recipients { get; set; }

		[Property]
		public double PaySum { get; set; }

		[Property]
		public DateTime PayDate { get; set; }

		[Property]
		public DateTime LastClientRegistrationDate { get; set; }
	
		[Property]
		public uint DisabledUsersCount { get; set; }

		[Property]
		public uint EnabledUsersCount { get; set; }

		[Property]
		public uint DisabledAddressesCount { get; set; }

		[Property]
		public uint EnabledAddressesCount { get; set; }
		
		[Property]
		public string Regions { get; set; }

		[Property]
		public bool HasWholesaleSegment { get; set; }

		[Property]
		public bool HasRetailSegment { get; set; }

		[Property]
		public bool ShowPayDate { get; set; }

		public bool IsDebitor()
		{
			return DateTime.Now - PayDate > TimeSpan.FromDays(1);
		}

		public bool IsDisabled
		{
			get { return (EnabledUsersCount == 0) || (EnabledAddressesCount == 0); }
		}

		public string GetSegments()
		{
			if (HasWholesaleSegment && HasRetailSegment)
				return "���, �������";
			if (HasWholesaleSegment)
				return "���";
			if (HasRetailSegment)
				return "�������";
			return "";
		}

		public static IList<BillingSearchItem> FindBy(BillingSearchProperties properties)
		{
			var sessionHolder = ActiveRecordMediator.GetSessionFactoryHolder();
			var session = sessionHolder.CreateSession(typeof(BillingSearchItem));
			properties.SearchText = Utils.StringToMySqlString(properties.SearchText);
			try
			{
				var debitorFilterBlock = "";
				var searchBlock = "";
				var groupFilter = "";

				switch(properties.SearchBy)
				{
					case SearchBy.Name:
						searchBlock = String.Format(
@"(p.ShortName like '{0}'
or p.JuridicalName like '{0}'
or sum(if(cd.Name like '{0}' or cd.FullName like '{0}', 1, 0)) > 0)", "%" + properties.SearchText + "%");
						break;
					case SearchBy.Code:
						searchBlock = String.Format("sum(if(cd.Id = {0}, 1, 0)) > 0", properties.SearchText);
						break;
					case SearchBy.UserId:
						searchBlock = String.Format("sum(if(users.Id = {0}, 1, 0)) > 0", properties.SearchText);
						break;
					case SearchBy.BillingCode:
						searchBlock = String.Format("p.payerId = {0}", properties.SearchText);
						break;
				}

				switch (properties.PayerState)
				{
					case PayerStateFilter.Debitors:
						debitorFilterBlock = "and p.oldpaydate <= curDate()";
						break;
					case PayerStateFilter.NotDebitors:
						debitorFilterBlock = "and p.oldpaydate > curDate()";
						break;
				}				

				switch(properties.Segment)
				{
					case SearchSegment.Retail:
						groupFilter = AddFilterCriteria(groupFilter, "cd.Segment = 1");
						break;
					case SearchSegment.Wholesale:
						groupFilter = AddFilterCriteria(groupFilter, "cd.Segment = 0");
						break;
				}

				switch(properties.ClientType)
				{
					case SearchClientType.Drugstore:
						groupFilter = AddFilterCriteria(groupFilter, "cd.FirmType = 1");
						break;
					case SearchClientType.Supplier:
						groupFilter = AddFilterCriteria(groupFilter, "cd.FirmType = 0");
						break;
				}

				switch(properties.ClientStatus)
				{
					case SearchClientStatus.Enabled:
						groupFilter = AddFilterCriteria(groupFilter, "cd.Status = 1");
						break;
					case SearchClientStatus.Disabled:
						groupFilter = AddFilterCriteria(groupFilter, "cd.Status = 0");
						break;
				}

				if (properties.RecipientId != 0)
				{
					groupFilter += AddFilterCriteria(groupFilter, " le.RecipientId = " + properties.RecipientId);
				}

				//.SetParameter("RegionId", properties.RegionId)
				if (properties.RegionId != UInt64.MaxValue)
					groupFilter = AddFilterCriteria(groupFilter, "cd.MaskRegion & :RegionId > 0");

				if (!String.IsNullOrEmpty(groupFilter))
					searchBlock = AddFilterCriteria(searchBlock, String.Format("sum(if({0}, 1, 0)) > 0", groupFilter));

				var query = session.CreateSQLQuery(String.Format(@"
select p.payerId as {{BillingSearchItem.BillingCode}},
		p.JuridicalName,
		p.shortname as {{BillingSearchItem.ShortName}},
		p.oldpaydate as {{BillingSearchItem.PayDate}},
		p.oldtariff as {{BillingSearchItem.PaySum}},
		max(cd.RegistrationDate) as {{BillingSearchItem.LastClientRegistrationDate}},
		count(distinct if(users.Enabled = 0, users.Id, null)) as {{BillingSearchItem.DisabledUsersCount}},
		count(distinct if(users.Enabled = 1, users.Id, null)) as {{BillingSearchItem.EnabledUsersCount}},
		count(distinct if(addresses.Enabled = 0, addresses.Id, null)) as {{BillingSearchItem.DisabledAddressesCount}},
		count(distinct if(addresses.Enabled = 1, addresses.Id, null)) as {{BillingSearchItem.EnabledAddressesCount}},

		not p.AutoInvoice as {{BillingSearchItem.ShowPayDate}},

		(select cast(group_concat(r.region order by r.region separator ', ') as char)
		from farm.regions r
		where r.regioncode & bit_or(cd.maskregion) > 0) as {{BillingSearchItem.Regions}},

		sum(if(cd.Segment = 1, 1, 0)) > 0 as {{BillingSearchItem.HasRetailSegment}},
		sum(if(cd.Segment = 0, 1, 0)) > 0 as {{BillingSearchItem.HasWholesaleSegment}},
		ifnull(group_concat(distinct ifnull(r.Name, '')), '') as {{BillingSearchItem.Recipients}}
from billing.payers p
	left join future.Users users on users.PayerId = p.PayerId
		left join future.Clients cd on cd.Id = users.ClientId
	left join future.Addresses addresses on addresses.PayerId = p.PayerId
		left join Billing.Recipients r on r.Id = p.RecipientId
where 1 = 1 {0}
group by p.payerId
having {1}
order by {{BillingSearchItem.ShortName}}
", debitorFilterBlock, searchBlock))
					.AddEntity(typeof(BillingSearchItem));

				if (properties.RegionId != UInt64.MaxValue)
					query.SetParameter("RegionId", properties.RegionId);

				var result = query.List<BillingSearchItem>();

				ArHelper.Evict(session, result);
				return result;
			}
			finally
			{
				sessionHolder.ReleaseSession(session);
			}
		}

		private static string AddFilterCriteria(string filter, string criteria)
		{
			if (String.IsNullOrEmpty(filter))
				return criteria;

			return filter + " and " + criteria;
		}

/*		public static IList<BillingSearchItem> FindBy2(BillingSearchProperties properties)
		{
			var sessionHolder = ActiveRecordMediator.GetSessionFactoryHolder();
			var session = sessionHolder.CreateSession(typeof(BillingSearchItem));
			try
			{
				AbstractCriterion groupFilter = Expression.Ge(Projections2.BitOr("cd.MaskRegion", 0), 0);

				switch (properties.Segment)
				{
					case SearchSegment.Retail:
						groupFilter = groupFilter & Expression.Eq("cd.Segment", Segment.Retail);
						break;
					case SearchSegment.Wholesale:
						groupFilter = groupFilter & Expression.Eq("cd.Segment", Segment.Wholesale);
						break;
				}

				switch (properties.ClientType)
				{
					case SearchClientType.Drugstore:
						groupFilter = groupFilter & Expression.Eq("cd.Segment", ClientType.Drugstore);
						break;
					case SearchClientType.Supplier:
						groupFilter = groupFilter & Expression.Eq("cd.Segment", ClientType.Supplier);
						break;
				}

				switch (properties.ClientStatus)
				{
					case SearchClientStatus.Enabled:
						groupFilter = groupFilter
									  & Expression.Eq("cd.Status", ClientStatus.On)
									  & Expression.Eq("cd.BillingStatus", ClientStatus.On);
						break;
					case SearchClientStatus.Disabled:
						groupFilter = groupFilter
									  & (Expression.Eq("cd.Status", ClientStatus.Off)
										 | Expression.Eq("cd.BillingStatus", ClientStatus.Off));
						break;
				}

				var criteria = session.CreateCriteria(typeof (Payer), "p")
					.CreateAlias("Clients", "cd")
					.SetProjection(
					Projections.ProjectionList()
						.Add(Projections.Property("p.PayerID"))
						.Add(Projections.Property("p.JuridicalName"))
						.Add(Projections.Property("p.ShortName"))
						.Add(Projections.Property("p.OldPayDate"))
						.Add(Projections.Property("p.OldTariff"))
						.Add(Projections.Max("cd.RegistrationDate"))
						.Add(Projections.Max("cd.Id"))
//						.Add(Projections.)
						.Add(Projections2.Sum(Projections.Conditional(Expression.Eq("cd.Status",
																					ClientStatus.Off),
																	  Projections.Constant(1),
																	  Projections.Constant(0))))
						.Add(Projections2.Sum(Projections.Conditional(Expression.Eq("cd.Status",
																					ClientStatus.On),
																	  Projections.Constant(1),
																	  Projections.Constant(0))))
						.Add(Projections2.Sum(Projections.Conditional(Expression.Eq("cd.Segment",
																					Segment.Wholesale),
																	  Projections.Constant(1),
																	  Projections.Constant(0))))
						.Add(Projections2.Sum(Projections.Conditional(Expression.Eq("cd.Segment",
																					Segment.Retail),
																	  Projections.Constant(1),
																	  Projections.Constant(0))))
						.Add(Projections.GroupProperty("p.PayerID"))
					)
					.AddOrder(Order.Asc("p.ShortName"));


				switch (properties.SearchBy)
				{
					case SearchBy.Name:
						criteria.Add(Restrictions.Like(Projections.GroupProperty("p.ShortName"),
													   properties.SearchText)
									 | Restrictions.Like(Projections.GroupProperty("p.JuridicalName"),
														  properties.SearchText)
									 | Restrictions.Ge(Projections2.Sum(Projections.Conditional(
																			Expression.Like("cd.FullName", properties.SearchText)
																			| Expression.Like("cd.ShortName", properties.SearchText),
																			Projections.Constant(1),
																			Projections.Constant(0))),
														0));
						break;
					case SearchBy.Code:
						criteria.Add(
							Restrictions.Ge(Projections2.Sum(Projections.Conditional(
																Expression.Eq("cd.id", properties.SearchText),
																Projections.Constant(1),
																Projections.Constant(0))), 0));
						break;
					case SearchBy.BillingCode:
						criteria.Add(Restrictions.Eq(Projections.GroupProperty("p.PayerID"), Convert.ToUInt32(properties.SearchText)));
						break;
				}


				switch (properties.PayerState)
				{
					case PayerStateFilter.Debitors:
						criteria.Add(Expression.Le("p.OldPayDate", DateTime.Now));
						break;
					case PayerStateFilter.NotDebitors:
						criteria.Add(Expression.Gt("p.OldPayDate", DateTime.Now));
						break;
				}

				criteria.Add(Expression.Ge(Projections2.Sum(Projections.Conditional(groupFilter,
																					Projections.Constant(1),
																					Projections.Constant(0))),
										   0));

				var s = criteria.List();

//                var result = session.CreateQuery(String.Format(@"
//select p.PayerID,
//		p.JuridicalName,
//        p.ShortName,
//        p.OldPayDate,
//        p.OldTariff,
//		max(cd.RegistrationDate),
//        max(cd.Id),
//        sum(if(cd.Status = 0, 1, 0)),
//        sum(if(cd.Status = 1, 1, 0)),
//
//		(select count(r.*) from Region r),
//
//		sum(if(cd.Segment = 1, 1, 0)),
//		sum(if(cd.Segment = 0, 1, 0))
//from Payer p
//	join p.Clients cd
//{0}
//group by p.PayerID
//", debitorFilterBlock, searchBlock, groupFilter))
////					.SetParameter("RegionId", properties.RegionId)
//                    .List();
				ArHelper.Evict(session, s);
				return null;
			}
			finally
			{
				sessionHolder.ReleaseSession(session);
			}
		}*/
	}
}