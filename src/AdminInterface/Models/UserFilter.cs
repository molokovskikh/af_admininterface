using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using AdminInterface.Helpers;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Common.MySql;
using Common.Tools;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using Common.Web.Ui.MonoRailExtentions;
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

	public class UserFilter : Sortable
	{
		public SearchUserBy SearchBy { get; set; }

		public string SearchText { get; set; }

		public SearchClientStatus SearchStatus { get; set; }

		public SearchSegment Segment { get; set; }

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
				{"Segment", "Segment"}
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
				var from = GetAdditionalFrom();
				filter = AddFilterCriteria(filter, GetFilterBy());
				filter = AddFilterCriteria(filter, GetSegmentFilter(Segment));
				filter = AddFilterCriteria(filter, GetTypeFilter(ClientType));
				filter = AddFilterCriteria(filter, GetStatusFilter(SearchStatus));
				if (!String.IsNullOrEmpty(filter))
					filter = String.Format(" and ({0}) ", filter);

				var regionMask = administrator.RegionMask;
				if (Region != null)
					regionMask &= Region.Id;

				var result = session.CreateSQLQuery(String.Format(@"
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
	r.Region as RegionName,
	if(sup.Segment is null, if(Clients.Segment = 0, 2, 1), if(sup.Segment = 0, 2, 1)) as Segment
FROM
	future.Users u
	join usersettings.UserUpdateInfo uui ON uui.UserId = u.Id
	join future.Services s on s.Id = u.RootService
		join farm.Regions r ON r.RegionCode = s.HomeRegion
	left join Future.Suppliers sup on sup.Id = u.RootService
	left join future.Clients ON Clients.Id = u.ClientId
		left join contacts.contact_groups cg ON cg.ContactGroupOwnerId = ifnull(Clients.ContactGroupOwnerId, sup.ContactGroupOwnerId)
		left join contacts.RegionalDeliveryGroups rdg on rdg.ContactGroupId = cg.Id
		left join contacts.Contacts ON Contacts.ContactOwnerId = cg.Id and if(rdg.ContactGroupId is not null, (rdg.RegionId & sup.RegionMask > 0), 1)
		left join contacts.Persons ON Persons.ContactGroupId = cg.Id and if(rdg.ContactGroupId is not null, (rdg.RegionId & sup.RegionMask > 0), 1)
	join Billing.Payers p on p.PayerId = u.PayerId
{2}
WHERE
	(r.RegionCode & :RegionMask > 0)
	{0}
GROUP BY u.Id
{1}
", filter, orderFilter, from))
					.SetParameter("RegionMask", regionMask)
					.ToList<UserSearchItem>();
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

		private string GetAdditionalFrom()
		{
			switch (SearchBy)
			{
				case SearchUserBy.AddressMail:
					return @"left join Future.UserAddresses ua on ua.UserId = u.Id
left join Future.Addresses a on a.Id = ua.AddressId";
					break;
				default:
					return "";
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

		private static string GetSegmentFilter(SearchSegment segment)
		{
			var filter = String.Empty;
			switch (segment)
			{
				case SearchSegment.Retail: {
					filter = AddFilterCriteria(filter, " (sup.Segment = 1 or Clients.Segment = 1) ");
					break;
				}
				case SearchSegment.Wholesale: {
					filter = AddFilterCriteria(filter, " (sup.Segment = 0 or Clients.Segment = 0)");
					break;
				}
			}
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

		private string GetFilterBy()
		{
			var filter = String.Empty;
			var searchText = String.IsNullOrEmpty(SearchText) ? String.Empty : Utils.StringToMySqlString(SearchText);

			var sqlSearchText = String.Format("%{0}%", searchText).ToLower();
			var searchTextIsNumber = new Regex("^\\d{1,10}$").IsMatch(searchText);
			var searchTextIsPhone = new Regex("^\\d{1,10}$").IsMatch(searchText.Replace("-", ""));

			switch (SearchBy)
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
				case SearchUserBy.AddressMail:
					filter = AddFilterCriteria(filter, String.Format(" (concat(a.Id, '@waybills.analit.net') like '{0}' or concat(a.Id, '@refused.analit.net') like '{0}') ", sqlSearchText));
					break;
			}
			return filter;
		}
	}
}
