using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using Common.MySql;

namespace AdminInterface.Models
{
	public enum SearchUserBy
	{
		[Description("Автоматически")]
		Auto,
		[Description("Код пользователя")]
		ByUserId,
		[Description("Логин пользователя")]
		ByLogin,
		[Description("Комментарий пользователя")]
		ByUserName,
		[Description("Имя клиента")]
		ByClientName,
		[Description("Юридическое имя")]
		ByJuridicalName,
		[Description("Код договора")]
		ByPayerId
	}

	public enum StatusStateFilter
	{
		[Description("Все")]
		All,
		[Description("Включенные")]
		Enabled,
		[Description("Отключенные")]
		Disabled
	}

	public class UserSearchProperties
	{
		private string _searchText;

		public SearchUserBy SearchBy { get; set; }

		public string SearchText { get { return _searchText; } set { _searchText = Utils.StringToMySqlString(value); } }

		public ulong PayerId { get; set; }

		public SearchClientStatus SearchStatus { get; set; }

		public SearchSegment Segment { get; set; }

		public ulong RegionId { get; set; }
	}
}
