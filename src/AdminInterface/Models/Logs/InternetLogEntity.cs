using System;
using System.Collections.Generic;
using Castle.ActiveRecord;
using Common.Web.Ui.Helpers;
using NHibernate.Criterion;

namespace AdminInterface.Models.Logs
{
	[ActiveRecord(Table = "logs.IOSInternetLog")]
	public class InternetLogEntity : ActiveRecordBase<InternetLogEntity>
	{
		private string _parameters;

		[PrimaryKey]
		public uint Id { get; set; }

		[Property]
		public uint BytesSent { get; set; }

		[Property]
		public string UserName { get; set; }

		[Property]
		public DateTime LogTime { get; set; }

		[Property]
		public string Target { get; set; }

		[Property]
		public string Parameters
		{
			get { return _parameters; }
			set { _parameters = value; }
		}

		public int GetBeginByteNubmer()
		{
			var pattern = "&RangeStart=";
			var patternIndex = _parameters.IndexOf(pattern);
			if (patternIndex > 0)
				return Convert.ToInt32(_parameters.Substring(patternIndex + pattern.Length,
				                                             _parameters.Length - patternIndex - pattern.Length));
			return 0;
		}

		public static IList<InternetLogEntity> GetUpdateSession(string userName, DateTime beginDate, uint updateId)
		{
			return ArHelper.WithSession<InternetLogEntity>(
				session => session
				           	.CreateCriteria(typeof (InternetLogEntity))
				           	.Add(Expression.Like("Target", "Handler", MatchMode.Anywhere))
				           	.Add(Expression.Like("Parameters", "id=" + updateId, MatchMode.Anywhere))
				           	.Add(Expression.Or(Expression.Like("UserName", userName, MatchMode.Exact),
				           	                   Expression.Like("UserName", @"ANALIT\\" + userName, MatchMode.Exact)))
				           	.Add(Expression.Ge("LogTime", beginDate))
				           	.AddOrder(Order.Desc("LogTime"))
				           	.SetMaxResults(10)
				           	.List<InternetLogEntity>());
		}
	}
}