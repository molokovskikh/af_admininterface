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
using System.Reflection;
using AdminInterface.AbstractModel;

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
				{ "CreatedOn", "CreatedOn" },
				{ "UpdateType", "UpdateType" },
				{ "Size", "Size" },
				{ "Version", "Version" },
				{ "Addition", "Addition" }
			};
		}

		/// <summary>
		/// Поиск логов для нового приложения analit-f
		/// </summary>
		/// <returns></returns>
		public IList<RequestLog> Find(ISession session)
		{
			SortBy = SortBy == "Addition" ? "CreatedOn" : SortBy; //У типа RequestLog нет поля Addition
			var logQuery = FillQuery<ClientAppLog>(BeginDate, EndDate, session);
			var requestQuery = FillQuery<RequestLog>(BeginDate, EndDate, session);

			if (User != null) {
				logQuery = GetUserLog(logQuery);
				requestQuery = GetUserLog(requestQuery)
					.OrderBy(string.Format("{0} {1}", SortBy, SortDirection));
			}
			else if (Client != null) {
				logQuery = GetClientLog<ClientAppLog>(session);
				requestQuery = GetClientLog<RequestLog>(session)
					.OrderBy(string.Format("{0} {1}", SortBy, SortDirection));
			}
			if (ErrorType == 1) {
				requestQuery = session.Query<RequestLog>().Where(i => i.ErrorType == 1);
			} else if (ErrorType == 0) {
				requestQuery = session.Query<RequestLog>().Where(i => i.IsCompleted && !i.IsFaulted && i.UpdateType == "MainController");
			}

			var logs = logQuery.ToList();

			var results = requestQuery.ToList();

			var connectedLogs = logs
				.Where(l => l.RequestToken != null && results.Any(x => x.RequestToken == l.RequestToken))
				.ToArray();

			results.Each(x => x.HaveLog = connectedLogs.Any(y => y.RequestToken == x.RequestToken));

			return results;
		}

		private IQueryable<T> FillQuery<T>(DateTime begin, DateTime end, ISession session) where T : IPersonLog
		{
				return session.Query<T>()
					.Where(x => x.CreatedOn > begin && x.CreatedOn < end);
		}

		private IQueryable<T> GetUserLog<T>(IQueryable<T> personLog) where T : IPersonLog
		{
			return personLog.Where(i => i.User == User);
		}

		private IQueryable<T> GetClientLog<T>(ISession session, bool a = false) where T : IPersonLog
		{
			return session.Query<T>().Where(i => i.User.Client == Client);
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