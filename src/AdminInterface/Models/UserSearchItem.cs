﻿using System;
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
		public bool Enabled { get; set; }

		[Property]
		public bool InvisibleOnFirm { get; set; }

		[Property]
		public string JuridicalName { get; set; }

		public bool IsLoginExists { get; set; }

		public bool IsLocked { get; set; }

		public bool IsDisabled { get; set; }

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
	Users.Id as {{UserSearchItem.UserId}},
	if (Clients.Status = 0, 0, Users.Enabled) as {{UserSearchItem.Enabled}},
	Clients.Id as {{UserSearchItem.ClientId}},
	Users.Login as {{UserSearchItem.Login}},
	Users.Name as {{UserSearchItem.UserName}},
	Users.PayerId as {{UserSearchItem.PayerId}},
	Clients.Name as {{UserSearchItem.ClientName}},
	Regions.Region as {{UserSearchItem.RegionName}},
	if (max(uui.UpdateDate) >= max(uui.UncommitedUpdateDate), uui.UpdateDate, uui.UncommitedUpdateDate) as {{UserSearchItem.UpdateDate}},
	if (uui.UpdateDate is not null, if (max(uui.UpdateDate) >= max(uui.UncommitedUpdateDate), 0, 1), 0) as {{UserSearchItem.UpdateIsUncommited}},
	max(uui.AFAppVersion) as {{UserSearchItem.AFVersion}},
	if(Clients.Segment = 1, 1, 2) as {{UserSearchItem.Segment}},
	rcs.InvisibleOnFirm as {{UserSearchItem.InvisibleOnFirm}},
	p.JuridicalName as {{UserSearchItem.JuridicalName}},
	1 as {{UserSearchItem.ClientType}}
FROM
	future.Users
	JOIN future.Clients ON Clients.Id = Users.ClientId
		LEFT JOIN contacts.contact_groups cg ON cg.ContactGroupOwnerId = Clients.ContactGroupOwnerId
		LEFT JOIN contacts.Contacts ON Contacts.ContactOwnerId = cg.Id
		LEFT JOIN contacts.Persons ON Persons.ContactGroupId = cg.Id
	JOIN farm.Regions ON Regions.RegionCode = Clients.RegionCode
	JOIN usersettings.RetClientsSet rcs ON rcs.ClientCode = Clients.Id
	JOIN billing.Payers p ON users.PayerId = p.PayerID
	LEFT JOIN usersettings.UserUpdateInfo uui ON uui.UserId = Users.Id
WHERE
	(Regions.RegionCode & :AdminRegionMask > 0) AND
	(Clients.RegionCode & :RegionId > 0)
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
						filter = AddFilterCriteria(filter, " Users.Enabled = 1 ");
						break;
					}
				case SearchClientStatus.Disabled: {
						filter = AddFilterCriteria(filter, " Users.Enabled = 0 ");
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
LOWER(Users.Login) like '{0}' or
LOWER(Users.Name) like '{0}' or
LOWER(Clients.Name) like '{0}' or
LOWER(Clients.FullName) like '{0}' or 
(LOWER(Contacts.ContactText) like '{0}' and Contacts.Type = 0) or
LOWER(Persons.Name) like '{0}' ",
								sqlSearchText));
						if (searchTextIsNumber)
							filter += String.Format(@" or Users.Id = {0} or Clients.Id = {0} ", searchText);
						if (searchTextIsPhone && searchText.Length >= 5)
							filter += String.Format(" or (REPLACE(Contacts.ContactText, '-', '') like '{0}' and Contacts.Type = 1) ",
								sqlSearchText.Replace("-", ""));
					break;
					}
				case SearchUserBy.ByClientName: {
						filter = AddFilterCriteria(filter,
							String.Format(" LOWER(Clients.Name) like '{0}' or LOWER(Clients.FullName) like '{0}' ", sqlSearchText));
						break;
					}
				case SearchUserBy.ByClientId: {
						filter = AddFilterCriteria(filter, String.Format(" Clients.Id = {0} ",
							searchTextIsNumber ? searchText : "-1"));
						break;
					}
				case SearchUserBy.ByJuridicalName: {
						filter = AddFilterCriteria(filter, String.Format(" LOWER(p.JuridicalName) like '{0}'", sqlSearchText));
						break;
					}
				case SearchUserBy.ByLogin: {
						filter = AddFilterCriteria(filter, String.Format(" LOWER(Users.Login) like '{0}' ", sqlSearchText));
						break;
					}
				case SearchUserBy.ByPayerId: {
						filter = AddFilterCriteria(filter, String.Format(" Users.PayerId = {0} ",
                            searchTextIsNumber ? searchText : "-1"));
						break;
					}
				case SearchUserBy.ByUserId: {
						filter = AddFilterCriteria(filter, String.Format(" Users.Id = {0} ", searchTextIsNumber ? searchText : "-1"));
						break;
					}
				case SearchUserBy.ByUserName: {
						filter = AddFilterCriteria(filter, String.Format(" LOWER(Users.Name) like '{0}' ", sqlSearchText));
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
