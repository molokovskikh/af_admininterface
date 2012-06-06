using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using AdminInterface.Helpers;
using AdminInterface.Models.Billing;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Common.MySql;
using Common.Tools;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using Common.Web.Ui.NHibernateExtentions;

namespace AdminInterface.Models
{
	public enum SearchUserBy
	{
		[Description("Автоматически")] Auto,
		[Description("Код клиента")] ByClientId,
		[Description("Код пользователя")] ByUserId,
		[Description("Логин пользователя")] ByLogin,
		[Description("Комментарий пользователя")] ByUserName,
		[Description("Email/телефон")] ByContacts,
		[Description("Контактное лицо (Ф.И.О.)")] ByPersons,
		[Description("Имя клиента")] ByClientName,
		[Description("Юридическое имя")] ByJuridicalName,
		[Description("Код договора")] ByPayerId,
		[Description("Адрес для отправки документов")] AddressMail
	}

	public enum StatusStateFilter
	{
		[Description("Все")] All,
		[Description("Включенные")] Enabled,
		[Description("Отключенные")] Disabled
	}

	public class SearchTextInfo
	{
		public string SearchText;
		public string SqlSearchText;
		public bool SearchTextIsNumber;
		public bool SearchTextIsPhone;

		public SearchTextInfo(string text)
		{
			SearchText = String.IsNullOrEmpty(text) ? String.Empty : Utils.StringToMySqlString(text);

			SqlSearchText = String.Format("%{0}%", SearchText).ToLower();
			SearchTextIsNumber = new Regex("^\\d{1,10}$").IsMatch(SearchText);
			SearchTextIsPhone = new Regex("^\\d{1,10}$").IsMatch(SearchText.Replace("-", ""));
		}
	}

	public class UserFilter : Sortable
	{
		public SearchUserBy SearchBy { get; set; }

		public string SearchText { get; set; }

		public SearchClientStatus SearchStatus { get; set; }

		public SearchClientType ClientType { get; set; }

		public Region Region { get; set; }

		public UserFilter()
		{
			SearchBy = SearchUserBy.Auto;
			SortBy = "UserName";
			SortKeyMap = new Dictionary<string, string> {
				{"PayerId", "PayerId"},
				{"ClientId", "ClientId"},
				{"UserId", "UserId"},
				{"ClientName", "ClientName"},
				{"Login", "Login"},
				{"UserName", "UserName"},
				{"RegionName", "RegionName"},
				{"UpdateDate", "UpdateDate"},
				{"AFVersion", "AFVersion"},
				{"ClientType", "ClientType"},
			};
		}

		public IList<UserSearchItem> Find()
		{
			var sessionHolder = ActiveRecordMediator.GetSessionFactoryHolder();
			var administrator = SecurityContext.Administrator;
			var session = sessionHolder.CreateSession(typeof(UserSearchItem));
			var orderFilter = String.Format("ORDER BY {0} {1}", GetSortProperty(), GetSortDirection());
			try
			{
				var filter = String.Empty;
				filter = AddFilterCriteria(filter, GetFilterBy());
				filter = AddFilterCriteria(filter, GetTypeFilter(ClientType));
				filter = AddFilterCriteria(filter, GetStatusFilter(SearchStatus));
				if (!String.IsNullOrEmpty(filter))
					filter = String.Format(" and ({0}) ", filter);

				var regionMask = administrator.RegionMask;
				if (Region != null)
					regionMask &= Region.Id;

				var sqlStr = String.Format(@"
SELECT
	u.Id as UserId,
	u.Login as Login,
	u.Name as UserName,
	u.PayerId as PayerId,
	u.Enabled as UserEnabled,
	if (max(uui.UpdateDate) >= max(uui.UncommitedUpdateDate), uui.UpdateDate, uui.UncommitedUpdateDate) as UpdateDate,
	if (uui.UpdateDate is not null, if (max(uui.UpdateDate) >= max(uui.UncommitedUpdateDate), 0, 1), 0) as UpdateIsUncommited,
	max(uui.AFAppVersion) as AFVersion,

	if(s.Type = 0, 2, 1) as ClientType,
	s.Id as ClientId,
	s.Name as ClientName,
	s.Disabled as ServiceDisabled,

	p.JuridicalName as JuridicalName,
	r.Region as RegionName
FROM
	Customers.Users u
	join usersettings.UserUpdateInfo uui ON uui.UserId = u.Id
	join Customers.Services s on s.Id = u.RootService
		join farm.Regions r ON r.RegionCode = s.HomeRegion
	left join Customers.Suppliers sup on sup.Id = u.RootService
	left join Customers.Clients ON Clients.Id = u.ClientId
	left join Customers.UserAddresses ua on ua.UserId = u.Id
	left join Customers.Addresses a on a.Id = ua.AddressId
		left join contacts.contact_groups cg ON cg.ContactGroupOwnerId = ifnull(Clients.ContactGroupOwnerId, sup.ContactGroupOwnerId)
		left join contacts.contact_groups cga ON a.ContactGroupId = cga.Id
		left join contacts.RegionalDeliveryGroups rdg on rdg.ContactGroupId = cg.Id
		left join contacts.RegionalDeliveryGroups rdga on rdga.ContactGroupId = cga.Id
		left join contacts.Contacts ON Contacts.ContactOwnerId = cg.Id and if(rdg.ContactGroupId is not null, (rdg.RegionId & sup.RegionMask > 0), 1)
		left join contacts.Contacts as ContactsAddresses ON ContactsAddresses.ContactOwnerId = cga.Id and if(rdga.ContactGroupId is not null, (rdga.RegionId & sup.RegionMask > 0), 1)
		left join contacts.Persons ON Persons.ContactGroupId = cg.Id and if(rdg.ContactGroupId is not null, (rdg.RegionId & sup.RegionMask > 0), 1)
	join Billing.Payers p on p.PayerId = u.PayerId
WHERE
	((Clients.MaskRegion & :RegionMask > 0) or (sup.RegionMask & :RegionMask > 0))
	{0}
GROUP BY u.Id
{1}
", filter, orderFilter);

				var result = session.CreateSQLQuery(sqlStr)
					.SetParameter("RegionMask", regionMask)
					.ToList<UserSearchItem>();
				ArHelper.Evict(session, result);
				var logins = result.Select(t => t.Login).ToList();

				var adInfo = ADHelper.GetPartialUsersInformation(logins);

				for (var i = 0; i < result.Count; i++)
				{
					if (adInfo == null)
						continue;
					result[i].IsDisabled = adInfo[result[i].Login].IsDisabled;
					result[i].IsLocked = adInfo[result[i].Login].IsLocked;
					result[i].IsLoginExists = adInfo[result[i].Login].IsLoginExists;
				}

				var info = new SearchTextInfo(SearchText);
				if ((info.SearchTextIsPhone && info.SearchText.Length >= 5) || SearchBy == SearchUserBy.ByContacts) {
					var findedUsers = result.Select(r => r.UserId).Implode();
					var findInUsers = session.CreateSQLQuery(string.Format(@"
select u.Id 
from Customers.Users u
left join Customers.UserAddresses ua on ua.UserId = u.Id
left join Customers.Addresses a on a.Id = ua.AddressId
left join contacts.contact_groups cg ON u.ContactGroupId = cg.Id
left join contacts.contact_groups cga ON a.ContactGroupId = cga.Id
left join Customers.Suppliers sup on sup.Id = u.RootService
left join Customers.Clients ON Clients.Id = u.ClientId
left join contacts.RegionalDeliveryGroups rdg on rdg.ContactGroupId = cg.Id
left join contacts.RegionalDeliveryGroups rdga on rdga.ContactGroupId = cga.Id
left join contacts.Contacts ON Contacts.ContactOwnerId = cg.Id and if(rdg.ContactGroupId is not null, (rdg.RegionId & sup.RegionMask > 0), 1)
left join contacts.Contacts as ContactAddress ON ContactAddress.ContactOwnerId = cga.Id and if(rdga.ContactGroupId is not null, (rdga.RegionId & sup.RegionMask > 0), 1)
where 
((Clients.MaskRegion & :RegionMask > 0) or (sup.RegionMask & :RegionMask > 0)) and
((REPLACE(Contacts.ContactText, '-', '') like '{0}' and Contacts.Type = 1) or
(REPLACE(Contacts.ContactText, '-', '') like '{0}' and Contacts.Type = 1))
and
u.Id in ({1})
", info.SqlSearchText.Replace("-", ""), findedUsers)).SetParameter("RegionMask", regionMask).List<uint>();

					if (findInUsers.Count > 0) { 
						var bufResult = result.Where(r => findInUsers.Contains(r.UserId)).ToList();
						result = bufResult;
					}
				}
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
				return ProcessFilter(criteria);
			if (String.IsNullOrEmpty(criteria))
				return ProcessFilter(filter);
			var newFilter = String.Format(" ({0}) and ({1}) ", filter, criteria);
			return ProcessFilter(newFilter);
		}

		private static string ProcessFilter(string filter)
		{
			if (filter.Contains('№'))
				filter = String.Format(" ({0}) or ({1}) ", filter, filter.Replace('№', 'N'));
			return filter;
		}

		private static string GetTypeFilter(SearchClientType type)
		{
			var filter = String.Empty;
			switch (type)
			{
				case SearchClientType.Drugstore:
					filter = AddFilterCriteria(filter, " s.Type = 1 ");
					break;
				case SearchClientType.Supplier:
					filter = AddFilterCriteria(filter, " s.Type = 0 ");
					break;
			}
			return filter;
		}

		private static string GetStatusFilter(SearchClientStatus status)
		{
			var filter = String.Empty;
			switch (status)
			{
				case SearchClientStatus.Enabled: {
					filter = AddFilterCriteria(filter, " u.Enabled = 1 and s.Disabled = 0");
					break;
				}
				case SearchClientStatus.Disabled: {
					filter = AddFilterCriteria(filter, " (u.Enabled = 0 or s.Disabled = 1)");
					break;
				}
			}
			return filter;
		}

		private string GetFilterBy()
		{
			var filter = String.Empty;

			var info = new SearchTextInfo(SearchText);
			var searchText = info.SearchText;

			var sqlSearchText = info.SqlSearchText;
			var searchTextIsNumber = info.SearchTextIsNumber;
			var searchTextIsPhone = info.SearchTextIsPhone;

			switch (SearchBy)
			{
				case SearchUserBy.Auto: {
					if (searchTextIsNumber)
						filter += String.Format(@" u.Id = {0} or s.Id = {0} ", searchText);
					else {
						filter = AddFilterCriteria(filter,
						String.Format(@"
LOWER(u.Login) like '{0}' or
LOWER(u.Name) like '{0}' or
LOWER(s.Name) like '{0}' ",
							sqlSearchText));
					}
					if (searchTextIsPhone && searchText.Length >= 5)
						filter += String.Format(" or (REPLACE(Contacts.ContactText, '-', '') like '{0}' and Contacts.Type = 1)" +
								" or (REPLACE(ContactsAddresses.ContactText, '-', '') like '{0}' and ContactsAddresses.Type = 1) ",
							sqlSearchText.Replace("-", ""));
					break;
				}
				case SearchUserBy.ByClientName: {
					filter = AddFilterCriteria(filter,
						String.Format(" LOWER(s.Name) like '{0}'", sqlSearchText));
					break;
				}
				case SearchUserBy.ByClientId: {
					filter = AddFilterCriteria(filter, String.Format(" s.Id = {0} ", searchTextIsNumber ? searchText : "-1"));
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
					filter = AddFilterCriteria(filter, String.Format(" u.PayerId = {0} ", searchTextIsNumber ? searchText : "-1"));
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
							String.Format(@"
(REPLACE(Contacts.ContactText, '-', '') like '{0}' and Contacts.Type = 1 ) or 
(REPLACE(ContactsAddresses.ContactText, '-', '') like '{0}' and ContactsAddresses.Type = 1 )
", sqlSearchText.Replace("-", "")));
					else
						filter = AddFilterCriteria(filter,
							String.Format(@"
(LOWER(Contacts.ContactText) like '{0}' and Contacts.Type = 0 and ((cg.Specialized = false) or (cg.id = ifnull(u.ContactGroupId, (cg.Specialized = false))))) or
(LOWER(ContactsAddresses.ContactText) like '{0}' and ContactsAddresses.Type = 0 and ((cga.Specialized = false) or (cga.id = ifnull(u.ContactGroupId, (cga.Specialized = false)))))
", sqlSearchText));
					break;
				}
				case SearchUserBy.ByPersons: {
					filter = AddFilterCriteria(filter, String.Format(" LOWER(Persons.Name) like '{0}' ", sqlSearchText));
					break;
				}
				case SearchUserBy.AddressMail:
					filter = AddFilterCriteria(filter, String.Format(" (concat(a.Id, '@waybills.analit.net') like '{0}' or concat(a.Id, '@refused.analit.net') like '{0}') ", sqlSearchText));
					break;
			}
			return filter;
		}
	}
}
