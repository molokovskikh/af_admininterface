using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Routing;
using AdminInterface.Models.Logs;
using Castle.ActiveRecord;
using Common.MySql;
using Common.Tools;
using Common.Web.Ui.Controllers;
using MySql.Data.MySqlClient;
using NHibernate;
using NHibernate.Linq;

namespace AdminInterface.Controllers
{
	public class RequestStat
	{
		public int Count { get; set; }
		public string Version { get; set; }
	}

	public class ErrorStat
	{
		public int Count { get; set; }
		public string Versions { get; set; }
		public string Users { get; set; }
		public string Error { get; set; }
		public string StackTrace { get; set; }
	}

	public class Log
	{
		//некоторые ошибки не интересны
		private string[] networkErrors = {
			"System.Net.WebException: Невозможно разрешить удаленное имя",
			"System.Net.WebException: The remote name could not be resolved",
			"System.Net.WebException: The operation has timed out.",
			"System.Net.Sockets.SocketException: Попытка установить соединение была безуспешной",
			"System.Net.Sockets.SocketException: Сделана попытка выполнить операцию на сокете при отключенной сети",
			"System.Net.Sockets.SocketException: Сделана попытка доступа к сокету методом, запрещенным правами доступа",
			"System.Net.Sockets.SocketException: Программа на вашем хост-компьютере разорвала установленное подключение",
			"System.Net.Sockets.SocketException: Удаленный хост принудительно разорвал существующее подключение",
			//todo временно до релиза
			"AnalitF.Net.Client.Models.Commands.RepairDb",
			//todo нужно подумать как быть с out of memory но пока игнорируем
			"System.OutOfMemoryException: Insufficient memory to continue the execution of the program.",
			"System.OutOfMemoryException: Недостаточно памяти для продолжения выполнения программы.",
			//todo думаю что это тоже out of memory но из mysql
			"Got error 134 from storage engine",
			//todo пока не понятно что с ним делать нужно осмыслить\добавить диагностику
			"System.OutOfMemoryException"
		};
		private Regex[] cleanup = {
			new Regex(@"\(HashCode=\d+\)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
			new Regex(@"токен = \S+", RegexOptions.Compiled | RegexOptions.IgnoreCase),
			new Regex(@"#PriceId: \d+, RegionId: \d+", RegexOptions.Compiled | RegexOptions.IgnoreCase),
		};

		public class Error
		{
			public string User;
			public string Version;
			public DateTime Date;
			public string Text;
			public string StackTrace;
		}

		public List<Error> Execute(DateTime begin, DateTime end, string[] versions)
		{
			var errors = new List<Error>();
			using(var connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["Main"].ConnectionString)) {
				connection.Open();
				var s = "";
				if (versions.Length > 0)
					s = $"and version in ({versions.Select(x => "'" + x + "'").Implode()})";
				var sql = $"select * from Logs.ClientAppLogs where CreatedOn > ?begin and CreatedOn < ?end and userId <> 758 {s} ";
				var records = connection.Read(sql, new { begin, end });
				foreach (var record in records) {
					var log = record["Text"].ToString();
					var reader = new StringReader(log);
					var messageHeader = new Regex(@"^\d{2}\.\d{2}\.\d{4} \d{2}:\d{2}:\d{2}\.\d{3} \[\d+\] (?<level>\w+) (?<text>.*)");
					Error lastError = null;
					string line;
					while ((line = reader.ReadLine()) != null) {
						var match = messageHeader.Match(line);
						if (match.Success) {
							if (match.Groups["level"].Value == "ERROR") {
								lastError = new Error {
									Version = record["Version"].ToString(),
									User = record["UserId"].ToString(),
									Text = match.Groups["text"].Value + "\r\n"
								};
								errors.Add(lastError);
							}
						} else if (lastError != null	) {
							if (!String.IsNullOrEmpty(lastError.StackTrace) || line.StartsWith("   at ") || line.StartsWith("   в "))
								lastError.StackTrace += line + "\r\n";
							else
								lastError.Text += line + "\r\n";
						}
					}
				}
			}

			errors = errors.Where(x => !networkErrors.Any(y => x.Text.Contains(y))).ToList();
			//некоторые ошибки могут быть одинаковыми по сути но содержать переменные данные
			//очищаем сообщения от таких данных
			foreach (var regex in cleanup) {
				errors.Each(x => x.Text = regex.Replace(x.Text, "<var>"));
			}
			return errors;
		}
	}

	public class MonController : Controller
	{
		protected ISession DbSession;

		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			var sessionHolder = ActiveRecordMediator.GetSessionFactoryHolder();
			DbSession = sessionHolder.CreateSession(typeof(ActiveRecordBase));
		}

		protected override void OnResultExecuted(ResultExecutedContext filterContext)
		{
			var sessionHolder = ActiveRecordMediator.GetSessionFactoryHolder();
			sessionHolder.ReleaseSession(DbSession);
		}

		public ActionResult Index()
		{
			var begin = DateTime.Today;
			var end = DateTime.Today.AddDays(1);
			var versions = new string[0];
			if (Request.HttpMethod == "POST") {
				begin = DateTime.Parse(Request.Form["begin"]);
				end = DateTime.Parse(Request.Form["end"]);
				versions = Request.Form.GetValues("version") ?? new string[0];
			}
			var query = FillVersions(begin, end, versions);

			if (versions.Length > 0) {
				query = query.Where(x => versions.Contains(x.Version));
			}

			var items = query
				.GroupBy(x => x.Version)
				.OrderByDescending(x => x.Count())
				.Select(x => new RequestStat { Count = x.Count(), Version = x.Key })
				.ToList();
			ViewBag.Begin = begin;
			ViewBag.End = end;
			ViewBag.Items = items;
			return View();
		}

		public ActionResult Users(string version, DateTime begin, DateTime end)
		{
			var items = DbSession.Query<RequestLog>()
				.Where(x => x.CreatedOn > begin && x.CreatedOn < end && x.Version == version)
				.OrderByDescending(x => x.CreatedOn)
				.ToList();
			return View(items);
		}

		public ActionResult Logs()
		{
			int threashold = 10;
			var begin = DateTime.Today;
			var end = DateTime.Today.AddDays(1);
			var versions = new string[0];
			if (Request.HttpMethod == "POST") {
				begin = DateTime.Parse(Request.Form["begin"]);
				end = DateTime.Parse(Request.Form["end"]);
				threashold = int.Parse(Request.Form["threashold"]);
				versions = Request.Form.GetValues("version") ?? new string[0];
			}
			FillVersions(begin, end, versions);
			ViewBag.Begin = begin;
			ViewBag.End = end;
			ViewBag.Threashold = threashold;
			var log = new Log();
			var errors = log.Execute(begin, end, versions);
			var items = errors.GroupBy(x => x.Text)
				.Select(x => new ErrorStat {
					Count = x.Count(),
					Versions = x.Select(y => y.Version).Distinct().Implode(),
					Users = x.Select(y => y.User).Distinct().Implode(),
					Error = x.Key,
					StackTrace = x.First().StackTrace
				})
				.OrderByDescending(x => x.Count)
				.Where(x => x.Count > threashold);
			return View(items);
		}

		private IQueryable<RequestLog> FillVersions(DateTime begin, DateTime end, string[] versions)
		{
			var query = DbSession.Query<RequestLog>()
				.Where(x => x.CreatedOn > begin && x.CreatedOn < end);
			ViewBag.Versions = query.GroupBy(x => x.Version).Select(x => x.Key).ToArray()
				.Select(x => new SelectListItem {
					Text = x,
					Value = x,
					Selected = versions.Contains(x)
				});
			return query;
		}
	}
}