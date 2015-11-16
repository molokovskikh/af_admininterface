using System;
using System.Collections.Generic;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using Common.Tools;
using Common.Web.Ui.Helpers;
using NHibernate;
using NHibernate.Linq;

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
		}

		/// <summary>
		/// Поиск логов для нового приложения analit-f
		/// </summary>
		/// <returns></returns>
		public IList<RequestLog> Find(ISession session)
		{
			var logQuery = session.Query<ClientAppLog>();
			var requestQuery = session.Query<RequestLog>();
			if (User != null) {
				logQuery = session.Query<ClientAppLog>().Where(i => i.User == User);
				requestQuery = session.Query<RequestLog>().Where(i => i.User == User);
			}
			else if (Client != null) {
				logQuery = session.Query<ClientAppLog>().Where(i => i.User.Client == Client);
				requestQuery = session.Query<RequestLog>().Where(i => i.User.Client == Client);
			}
			if (ErrorType == 1) {
				requestQuery = session.Query<RequestLog>().Where(i => i.ErrorType == 1);
			} else if (ErrorType == 0) {
				requestQuery = session.Query<RequestLog>().Where(i => i.IsCompleted && !i.IsFaulted && i.UpdateType == "MainController");
			}

			var logs = logQuery.Where(i => i.CreatedOn > BeginDate && i.CreatedOn < EndDate.AddDays(1)).ToList();

			var results = requestQuery
				.Where(i => i.CreatedOn > BeginDate && i.CreatedOn < EndDate.AddDays(1))
				.OrderByDescending(r => r.CreatedOn)
				.ToList();

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