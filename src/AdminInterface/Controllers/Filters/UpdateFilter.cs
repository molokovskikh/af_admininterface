using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using AdminInterface.Security;
using Common.MySql;
using Common.Tools;
using Common.Web.Ui.ActiveRecordExtentions;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using Common.Web.Ui.NHibernateExtentions;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;
using NHibernate.SqlCommand;
using NPOI.SS.Formula.Functions;
using log4net;

namespace AdminInterface.Controllers.Filters
{
	public class UpdateFilter : Sortable
	{
		public UpdateType? UpdateType { get; set; }
		public ulong RegionMask { get; set; }
		public DateTime BeginDate { get; set; }
		public DateTime EndDate { get; set; }
		public Client Client { get; set; }
		public User User { get; set; }
		public ILog logger;

		public UpdateFilter()
		{
			SortDirection = "Desc";
			SortKeyMap = new Dictionary<string, string> {
				{ "RequestTime", "RequestTime" },
				//хак, тк HomeRegion перегружен хибер не может разобраться по какому полю сортировать
				//по этому сортировка будет в ручную
				{ "Region", "Id" },
				{ "ClientName", "c.Name" },
				{ "UpdateType", "UpdateType" },
				{ "Login", "u.Login" },
				{ "ResultSize", "ResultSize" },
				{ "Addition", "Addition" },
				{ "AppVersion", "AppVersion" }
			};
			logger = LogManager.GetLogger(typeof(UpdateFilter));
		}

		public IList<UpdateLogView> Find(ISession s)
		{
			var projectionsList = new List<IProjection> {
				Projections.Property("Id").As("Id"),
				Projections.Property("Addition").As("Addition"),
				Projections.Property("Commit").As("Commit"),
				Projections.Property("AppVersion").As("AppVersion"),
				Projections.Property("ResultSize").As("ResultSize"),
				Projections.Property("UpdateType").As("UpdateType"),
				Projections.Property("RequestTime").As("RequestTime"),
				Projections.Alias(Projections.Conditional(Restrictions.IsNotNull("Log"), Projections.Constant(1), Projections.Constant(0)), "HaveLog"),
				Projections.Property("u.Id").As("UserId"),
				Projections.Property("u.Name").As("UserName"),
				Projections.Property("u.Login").As("Login"),
				Projections.Property("c.Id").As("ClientId"),
				Projections.Property("c.Name").As("ClientName"),
				Projections.SqlProjection("c2_.RegionCode as RegionId", new[] { "RegionId" }, new[] { NHibernateUtil.String })
			};

			if (UpdateType == Models.Logs.UpdateType.AccessError) {
				projectionsList.Add(Projections.ProjectionList().Add(Projections.Alias(Projections.SubQuery(DetachedCriteria.For<UpdateLogEntity>("ule")
					.Add(Restrictions.EqProperty("ue.User", "ule.User"))
					.Add(Restrictions.Eq("ule.Commit", true))
					.Add(Restrictions.In("ule.UpdateType", new object[] {
						Models.Logs.UpdateType.Accumulative,
						Models.Logs.UpdateType.Cumulative,
						Models.Logs.UpdateType.LimitedCumulative,
						Models.Logs.UpdateType.AutoOrder,
						Models.Logs.UpdateType.LoadingDocuments
					}))
					.Add(Restrictions.GtProperty("ule.RequestTime", "ue.RequestTime"))
					.SetProjection(Projections.ProjectionList()
						.Add(Projections.Conditional(Restrictions.Gt(
							Projections.Count(Projections.Property("ule.Id")), 0),
							Projections.Constant(1),
							Projections.Constant(0))))),
					"OkUpdate")));
			}

			var criteria = s.CreateCriteria<UpdateLogEntity>("ue")
				.CreateAlias("User", "u", JoinType.InnerJoin)
				.CreateAlias("u.Client", "c", JoinType.LeftOuterJoin)
				.SetProjection(projectionsList.ToArray());

			if (User != null)
				criteria.Add(Restrictions.Eq("User", User));

			if (Client != null)
				criteria.Add(Restrictions.Eq("u.Client", Client));

			if (UpdateType != null)
				criteria.Add(Restrictions.Eq("UpdateType", UpdateType));

			criteria
				.Add(Restrictions.Ge("RequestTime", BeginDate))
				.Add(Restrictions.Le("RequestTime", EndDate.AddDays(1)));

			var regionMask = RegionMask;
			if (regionMask == 0)
				regionMask = SecurityContext.Administrator.RegionMask;
			else
				regionMask = RegionMask & SecurityContext.Administrator.RegionMask;

			criteria.Add(Restrictions.Gt(Projections2.BitOr("u.WorkRegionMask", regionMask), 0));
			ApplySort(criteria);

			if (SortBy != "RequestTime")
				criteria.AddOrder(Order.Desc("RequestTime"));

			var items = criteria.ToList<UpdateLogView>();

			var regions = Region.All();
			items.Where(i => i.RegionId != null).Each(i => {
				var region = regions.FirstOrDefault(r => r.Id == i.RegionId.Value);
				if (region == null)
					return;
				i.Region = region.Name;
			});

			if (String.Equals(SortBy, "Region", StringComparison.OrdinalIgnoreCase)) {
				if (IsDesc())
					items = items.OrderByDescending(i => i.Region).ToList();
				else
					items = items.OrderBy(i => i.Region).ToList();

				var bufList = new List<UpdateLogView>();
				var groupItems = items.GroupBy(i => i.Region);
				groupItems.Each(g => bufList.AddRange(g.OrderByDescending(i => i.RequestTime).ToList()));
				items = bufList;
			}
			return items;
		}

		public bool ShowRegion()
		{
			return UpdateType != null;
		}

		public bool ShowClient()
		{
			return ShowRegion();
		}

		public bool ShowUpdateType()
		{
			return User != null || Client != null;
		}

		private bool IsDataTransferUpdate()
		{
			return UpdateType != null
				&& UpdateLogEntity.IsDataTransferUpdateType(UpdateType.Value);
		}

		public bool ShowUser()
		{
			return Client != null || UpdateType != null;
		}

		public bool ShowUpdateSize()
		{
			return ShowUpdateType();
		}

		public bool ShowLog()
		{
			return ShowUpdateType();
		}


		/// <summary>
		/// Создает объект лога для вывода на экран из специфического объекта лога нового клиентского приложения
		/// </summary>
		/// <param name="log">Объект лога клиентского приложения</param>
		/// <param name="additionLength">Максимальная длина описания</param>
		/// <returns></returns>
		public UpdateLogView CreateViewFromClientAppLog(ClientAppLog log, int additionLength)
		{
			var view = new UpdateLogView();
			view.Id = log.Id;
			view.ClientId = log.User.Client.Id;
			view.ClientName = log.User.Client.Name;
			view.AppVersion = log.Version;
			view.RequestTime = log.CreatedOn;
			view.Addition = log.Text;
			view.Type = 1;
			if (log.Text.Length > additionLength) {
				view.Addition = log.Text.Substring(0, additionLength) + "...";
				view.HaveLog = true;
			}
			return view;
		}

		/// <summary>
		/// Поиск логов для нового приложения analit-f
		/// </summary>
		/// <returns></returns>
		public IList<UpdateLogView> FindNewAppLogs(ISession DbSession)
		{
			var result = new List<UpdateLogView>();
			//длина отображаемого сообщения
			var additionLength = 150; 
			//Сначала находим клиентские записи
			//Забираем данные за месяц от указанной даты, так как мы не знаем когда клиент нам их отправил,
			//а указана лишь дата получения
			var logs = DbSession.Query<ClientAppLog>().Where(i => (i.User.Client == Client || i.User == User)
				&& i.CreatedOn > BeginDate.AddMonths(-1) && i.CreatedOn < EndDate.AddMonths(1)).ToList();
			foreach (var log in logs) {
				var view = new UpdateLogView();
				view.Id = log.Id;
				view.ClientId = log.User.Client.Id;
				view.ClientName = log.User.Client.Name;
				view.UserId = log.User.Id;
				view.UserName = log.User.Name;
				view.AppVersion = log.Version;
				view.RequestTime = log.CreatedOn;
				view.Addition = log.Text;
				view.Commit = true;
				view.Type = 1;
				view.UpdateType = Models.Logs.UpdateType.NoType;
				if (log.Text.Length > additionLength) {
					view.Addition = log.Text.Substring(0, additionLength) + "...";
					view.HaveLog = true;
				}
				result.Add(view);
			}

			//Теперь имщем серверные записи
			var rlogs = DbSession.Query<RequestLog>().Where(i => (i.User.Client == Client || i.User == User) && i.CreatedOn > BeginDate && i.CreatedOn < EndDate).ToList();
			foreach (var log in rlogs) {
				var view = new UpdateLogView();
				view.Id = log.Id;
				view.ClientId = log.User.Client.Id;
				view.UserId = log.User.Id;
				view.UserName = log.User.Name;
				view.ClientName = log.User.Client.Name;
				view.AppVersion = log.Version;
				view.RequestTime = log.CreatedOn;
				view.Addition = log.Error;
				view.Commit = log.IsConfirmed;
				view.HaveLog = false;
				view.Type = 2;
				view.UpdateType = Models.Logs.UpdateType.NoType;
				//Парсим тип из строки
				UpdateType type = view.UpdateType;
				if (Enum.TryParse(log.UpdateType, true, out type))
					view.UpdateType = type;

				result.Add(view);
			}

			//Сортировки нет, так как я нашел серьезный баг в сортировке стандартных логов, который годами не замечался
			//Из чего я делаю вывод, что сортировка тут вовсе не нужна - по крайней мере пока
			result = result.OrderBy(i => i.RequestTime).ToList();
			return result;
		}
	}

	public class UpdateLogView
	{
		/// <summary>
		///0 -лог аналитf
		///1 - клиентские логи новой версии
		///2 - серверные логи новой версии
		/// </summary>
		public int Type = 0;

		private string _username;

		public uint Id { get; set; }

		public string Addition { get; set; }

		public bool Commit { get; set; }

		public string AppVersion { get; set; }

		public DateTime RequestTime { get; set; }

		public uint ResultSize { get; set; }

		public UpdateType UpdateType { get; set; }

		public uint UserId { get; set; }

		public string Login { get; set; }

		public string UserName
		{
			get
			{
				if (String.IsNullOrEmpty(_username))
					return Login;
				return _username;
			}
			set { _username = value; }
		}

		public uint? ClientId { get; set; }

		public string ClientName { get; set; }

		public ulong? RegionId { get; set; }

		public string Region { get; set; }

		public bool HaveLog { get; set; }

		public bool OkUpdate { get; set; }

		public bool IsDataTransferUpdateType()
		{
			return UpdateLogEntity.IsDataTransferUpdateType(UpdateType);
		}
	}
}