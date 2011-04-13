using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AdminInterface.Helpers;
using AdminInterface.Models.Security;
using Castle.ActiveRecord;
using Common.Web.Ui.Helpers;
using Common.MySql;

namespace AdminInterface.Models
{
	[ActiveRecord(SchemaAction = "none")]
	public class UserSearchItem : ActiveRecordBase<UserSearchItem>
	{
		[PrimaryKey]
		public uint UserId { get; set; }

		[Property]
		public uint PayerId { get; set; }

		[Property]
		public uint ClientId { get; set; }

		[Property]
		public string Login { get; set; }

		[Property]
		public string UserName { get; set; }

		[Property]
		public string ClientName { get; set; }

		[Property]
		public string RegionName { get; set; }

		[Property]
		public uint AFVersion { get; set; }

		[Property]
		public SearchSegment Segment { get; set; }

		[Property]
		public SearchClientType ClientType { get; set; }

		[Property]
		public DateTime? UpdateDate { get; set; }

		[Property]
		public bool UpdateIsUncommited { get; set; }

		[Property]
		public bool Disabled { get; set; }

		[Property]
		public string JuridicalName { get; set; }

		public bool IsLoginExists { get; set; }

		public bool IsLocked { get; set; }

		public bool IsDisabled { get; set; }

		public bool IsDrugstore
		{
			get
			{
				return ClientType == SearchClientType.Drugstore;
			}
		}

		private static string ProcessFilter(string filter)
		{
			if (filter.Contains('№'))
				filter = String.Format(" ({0}) or ({1}) ", filter, filter.Replace('№', 'N'));
			return filter;
		}

		private static string AddFilterCriteria(string filter, string criteria)
		{
			if (String.IsNullOrEmpty(filter))
				return ProcessFilter(criteria);
			if (String.IsNullOrEmpty(criteria))
				return ProcessFilter(filter);
			var newFilter = String.Format(" ({0}) and ({1}) ", filter, criteria);
			return ProcessFilter(newFilter);
		}

		public static IList<UserSearchItem> SearchBy(Administrator administrator, UserSearchProperties searchProperties, string sortColumn, string sortDirection)
		{
			var sessionHolder = ActiveRecordMediator.GetSessionFactoryHolder();
			var session = sessionHolder.CreateSession(typeof(UserSearchItem));
			var orderDirection = sortDirection.Equals("Ascending", StringComparison.InvariantCultureIgnoreCase) ? "ASC" : "DESC";
			var orderFilter = String.Format("ORDER BY {{UserSearchItem.{0}}} {1}", sortColumn, orderDirection);
			try
			{
				var filter = String.Empty;

				filter = AddFilterCriteria(filter, GetFilterBy(searchProperties));
				filter = AddFilterCriteria(filter, GetSegmentFilter(searchProperties.Segment));
				filter = AddFilterCriteria(filter, GetStatusFilter(searchProperties.SearchStatus));
				if (!String.IsNullOrEmpty(filter))
					filter = String.Format(" and ({0}) ", filter);
				var result = session.CreateSQLQuery(String.Format(@"
SELECT
	u.Id as {{UserSearchItem.UserId}},
	u.Login as {{UserSearchItem.Login}},
	u.Name as {{UserSearchItem.UserName}},
	u.PayerId as {{UserSearchItem.PayerId}},
	if (max(uui.UpdateDate) >= max(uui.UncommitedUpdateDate), uui.UpdateDate, uui.UncommitedUpdateDate) as {{UserSearchItem.UpdateDate}},
	if (uui.UpdateDate is not null, if (max(uui.UpdateDate) >= max(uui.UncommitedUpdateDate), 0, 1), 0) as {{UserSearchItem.UpdateIsUncommited}},
	max(uui.AFAppVersion) as {{UserSearchItem.AFVersion}},

	s.Type as {{UserSearchItem.ClientType}},
	s.Id as {{UserSearchItem.ClientId}},
	s.Name as {{UserSearchItem.ClientName}},
	s.Disabled as {{UserSearchItem.Disabled}},

	p.JuridicalName as {{UserSearchItem.JuridicalName}},
	r.Region as {{UserSearchItem.RegionName}},
	0 as {{UserSearchItem.Segment}}
FROM
	future.Users u
	join usersettings.UserUpdateInfo uui ON uui.UserId = u.Id
	join future.Services s on s.Id = u.RootService
		join farm.Regions r ON r.RegionCode = s.HomeRegion
	left JOIN future.Clients ON Clients.Id = u.ClientId
		left JOIN contacts.contact_groups cg ON cg.ContactGroupOwnerId = Clients.ContactGroupOwnerId
		left JOIN contacts.Contacts ON Contacts.ContactOwnerId = cg.Id
		LEFT JOIN contacts.Persons ON Persons.ContactGroupId = cg.Id
	join Billing.Payers p on p.PayerId = u.PayerId
WHERE
	(r.RegionCode & :AdminRegionMask & :RegionId > 0)
	{0}
GROUP BY {{UserSearchItem.UserId}}
{1}
", filter, orderFilter))
						.AddEntity(typeof (UserSearchItem))
						.SetParameter("RegionId", searchProperties.RegionId)
						.SetParameter("AdminRegionMask", administrator.RegionMask)
						.List<UserSearchItem>();
				ArHelper.Evict(session, result);
				var logins = new List<string>();
				for (var i = 0; i < result.Count; i++)
					logins.Add(result[i].Login);

				var adInfo = ADHelper.GetPartialUsersInformation(logins);

				for (var i = 0; i < result.Count; i++)
				{
					if (adInfo == null)
						continue;
					result[i].IsDisabled = adInfo[result[i].Login].IsDisabled;
					result[i].IsLocked = adInfo[result[i].Login].IsLocked;
					result[i].IsLoginExists = adInfo[result[i].Login].IsLoginExists;
				}
				return result;
			}
			finally
			{
				sessionHolder.ReleaseSession(session);
			}
		}

		private static string GetSegmentFilter(SearchSegment segment)
		{
			var filter = String.Empty;
			switch (segment)
			{
				case SearchSegment.Retail: {
						filter = AddFilterCriteria(filter, " Clients.Segment = 1 ");
						break;
					}
				case SearchSegment.Wholesale: {
						filter = AddFilterCriteria(filter, " Clients.Segment = 0 ");
						break;
					}
			}
			return filter;
		}

		private static string GetStatusFilter(SearchClientStatus status)
		{
			var filter = String.Empty;
			switch (status)
			{
				case SearchClientStatus.Enabled: {
						filter = AddFilterCriteria(filter, " u.Enabled = 1 ");
						break;
					}
				case SearchClientStatus.Disabled: {
						filter = AddFilterCriteria(filter, " u.Enabled = 0 ");
						break;
					}
			}
			return filter;
		}

		private static string GetFilterBy(UserSearchProperties searchProperties)
		{
			var filter = String.Empty;
			var searchText = String.IsNullOrEmpty(searchProperties.SearchText) ? String.Empty :
				Utils.StringToMySqlString(searchProperties.SearchText);

			var sqlSearchText = String.Format("%{0}%", searchText).ToLower();
			var searchTextIsNumber = new Regex("^\\d{1,10}$").IsMatch(searchText);
			var searchTextIsPhone = new Regex("^\\d{1,10}$").IsMatch(searchText.Replace("-", ""));

			switch (searchProperties.SearchBy)
			{
				case SearchUserBy.Auto: {
						filter = AddFilterCriteria(filter,
							String.Format(@"
LOWER(u.Login) like '{0}' or
LOWER(u.Name) like '{0}' or
LOWER(s.Name) like '{0}' or
(LOWER(Contacts.ContactText) like '{0}' and Contacts.Type = 0) or
LOWER(Persons.Name) like '{0}' ",
								sqlSearchText));
						if (searchTextIsNumber)
							filter += String.Format(@" or u.Id = {0} or s.Id = {0} ", searchText);
						if (searchTextIsPhone && searchText.Length >= 5)
							filter += String.Format(" or (REPLACE(Contacts.ContactText, '-', '') like '{0}' and Contacts.Type = 1) ",
								sqlSearchText.Replace("-", ""));
					break;
					}
				case SearchUserBy.ByClientName: {
						filter = AddFilterCriteria(filter,
							String.Format(" LOWER(s.Name) like '{0}'", sqlSearchText));
						break;
					}
				case SearchUserBy.ByClientId: {
						filter = AddFilterCriteria(filter, String.Format(" s.Id = {0} ",
							searchTextIsNumber ? searchText : "-1"));
						break;
					}
				case SearchUserBy.ByJuridicalName: {
						filter = AddFilterCriteria(filter, String.Format(" LOWER(p.JuridicalName) like '{0}'", sqlSearchText));
						break;
					}
				case SearchUserBy.ByLogin: {
						filter = AddFilterCriteria(filter, String.Format(" LOWER(u.Login) like '{0}' ", sqlSearchText));
						break;
					}
				case SearchUserBy.ByPayerId: {
						filter = AddFilterCriteria(filter, String.Format(" u.PayerId = {0} ",
                            searchTextIsNumber ? searchText : "-1"));
						break;
					}
				case SearchUserBy.ByUserId: {
						filter = AddFilterCriteria(filter, String.Format(" u.Id = {0} ", searchTextIsNumber ? searchText : "-1"));
						break;
					}
				case SearchUserBy.ByUserName: {
						filter = AddFilterCriteria(filter, String.Format(" LOWER(u.Name) like '{0}' ", sqlSearchText));
						break;
					}
				case SearchUserBy.ByContacts: {
						if (searchTextIsPhone || searchTextIsNumber)
							filter = AddFilterCriteria(filter,
								String.Format(" REPLACE(Contacts.ContactText, '-', '') like '{0}' and Contacts.Type = 1 ", sqlSearchText.Replace("-", "")));
						else
							filter = AddFilterCriteria(filter,
								String.Format(" LOWER(Contacts.ContactText) like '{0}' and Contacts.Type = 0 ", sqlSearchText));
						break;
					}
				case SearchUserBy.ByPersons: {
						filter = AddFilterCriteria(filter, String.Format(" LOWER(Persons.Name) like '{0}' ", sqlSearchText));
						break;
					}
			}
			return filter;
		}
	}
}
