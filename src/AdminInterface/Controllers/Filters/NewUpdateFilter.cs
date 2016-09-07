using System;
using System.Collections.Generic;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using Common.Tools;
using Common.Web.Ui.Helpers;
using NHibernate;
using NHibernate.Linq;
using System.Linq.Dynamic;

namespace AdminInterface.Controllers.Filters
{
	public class NewUpdateFilter : Sortable
	{
		public ulong? RegionMask { get; set; }
		public DateTime BeginDate { get; set; }
		public DateTime EndDate { get; set; }
		public int? ErrorType { get; set; }
		public Client Client { get; set; }
		public User User { get; set; }

		public NewUpdateFilter()
		{
			BeginDate = DateTime.Today;
			EndDate = DateTime.Today.AddDays(1);
			SortBy = "CreatedOn";
			SortDirection = "Desc";
			SortKeyMap = new Dictionary<string, string> {
				{ "User", "User" },
				{ "CreatedOn", "CreatedOn" },
				{ "UpdateType", "UpdateType" },
				{ "Size", "Size" },
				{ "Version", "Version" },
				{ "RemoteHost", "RemoteHost" },
				{ "LocalHost", "LocalHost" },
				{ "OSVersion", "OSVersion" },
			};
		}

		/// <summary>
		/// Поиск логов для нового приложения analit-f
		/// </summary>
		/// <returns></returns>
		public IList<RequestLog> Find(ISession session)
		{
			var logQuery = session.Query<ClientAppLog>()
					.Where(x => x.CreatedOn > BeginDate && x.CreatedOn < EndDate.AddDays(1));
			var requestQuery = session.Query<RequestLog>()
					.Where(x => x.CreatedOn > BeginDate && x.CreatedOn < EndDate.AddDays(1))
					.OrderBy(string.Format("{0} {1}", SortBy, SortDirection));

			if (User != null) {
				logQuery = logQuery.Where(i => i.User == User);
				requestQuery = requestQuery.Where(i => i.User == User);
			}
			else if (Client != null) {
				logQuery = logQuery.Where(i => i.User.Client == Client);
				requestQuery = requestQuery.Where(i => i.User.Client == Client);
			}
			if (ErrorType == 1) {
				requestQuery = session.Query<RequestLog>().Where(i => i.ErrorType == 1 && i.CreatedOn > BeginDate && i.CreatedOn < EndDate.AddDays(1))
					.OrderBy(string.Format("{0} {1}", SortBy, SortDirection));
			}
			else if (ErrorType == 0) {
				requestQuery = session.Query<RequestLog>()
					.Where(i => i.IsCompleted && !i.IsFaulted && i.UpdateType == "MainController" && i.CreatedOn > BeginDate && i.CreatedOn < EndDate.AddDays(1))
					.OrderBy(string.Format("{0} {1}", SortBy, SortDirection));
			}

			var logs = logQuery.ToList();

			var results = requestQuery.ToList();

			var connectedLogs = logs
				.Where(l => l.RequestToken != null && results.Any(x => x.RequestToken == l.RequestToken))
				.ToArray();

			results.Each(x => x.HaveLog = connectedLogs.Any(y => y.RequestToken == x.RequestToken));

			return results;
		}

		public bool ShowRegion()
		{
			return ErrorType != null;
		}

		public bool ShowClient()
		{
			return ShowRegion();
		}

		public bool ShowUpdateType()
		{
			return User != null || Client != null;
		}

		public bool ShowUser()
		{
			return Client != null || ErrorType != null;
		}

		public bool ShowUpdateSize()
		{
			return ShowUpdateType();
		}

		public bool ShowLog()
		{
			return ShowUpdateType();
		}
	}
}