using System;
using System.Collections.Generic;
using Castle.ActiveRecord;
using Common.Web.Ui.Helpers;
using NHibernate;
using NHibernate.Expression;

namespace AdminInterface.Model
{
	[ActiveRecord(Table = "logs.IOSInternetLog")]
	public class InternetLogEntity : ActiveRecordBase<InternetLogEntity>
	{
		private uint _id;
		private uint _bytesSent;
		private string _userName;
		private string _target;
		private string _parameters;
		private DateTime _logTime;

		[PrimaryKey]
		public uint Id
		{
			get { return _id; }
			set { _id = value; }
		}

		[Property]
		public uint BytesSent
		{
			get { return _bytesSent; }
			set { _bytesSent = value; }
		}

		[Property]
		public string UserName
		{
			get { return _userName; }
			set { _userName = value; }
		}

		[Property]
		public DateTime LogTime
		{
			get { return _logTime; }
			set { _logTime = value; }
		}

		[Property]
		public string Target
		{
			get { return _target; }
			set { _target = value; }
		}

		[Property]
		public string Parameters
		{
			get { return _parameters; }
			set { _parameters = value; }
		}

		public int GetBeginByteNubmer()
		{
			string pattern = "&RangeStart=";
			int patternIndex = _parameters.IndexOf(pattern);
			if (patternIndex > 0)
				return
					Convert.ToInt32(_parameters.Substring(patternIndex + pattern.Length, _parameters.Length - patternIndex - pattern.Length));
			else
				return 0;
		}

		public static IList<InternetLogEntity> GetUpdateSession(string userName, DateTime beginDate, uint updateId)
		{
			return ArHelper.WithSession<InternetLogEntity>(
				delegate(ISession session)
					{
						return session
							.CreateCriteria(typeof (InternetLogEntity))
								.Add(Expression.Like("Target", "Handler", MatchMode.Anywhere))
								.Add(Expression.Like("Parameters", "id=" + updateId, MatchMode.Anywhere))
								.Add(Expression.Or(Expression.Like("UserName", userName, MatchMode.Exact), 
												   Expression.Like("UserName", @"ANALIT\\" + userName, MatchMode.Exact)))
								.Add(Expression.Ge("LogTime", beginDate))
								.AddOrder(Order.Desc("LogTime"))
								.SetMaxResults(10)
								.List<InternetLogEntity>();
					});
		}
	}
}