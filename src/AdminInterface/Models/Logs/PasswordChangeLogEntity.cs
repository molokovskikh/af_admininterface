using System;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Castle.ActiveRecord;
using NHibernate.Expression;

namespace AdminInterface.Models.Logs
{
	[ActiveRecord("logs.passwordchange")]
	public class PasswordChangeLogEntity
	{
		private uint _id;
		private string _clientHost;
		private DateTime _logTime;
		private string _userName;
		private string _targetUserName;

		[PrimaryKey("RowId")]
		public uint Id
		{
			get { return _id; }
			set { _id = value; }
		}

		[Property]
		public string ClientHost
		{
			get { return _clientHost; }
			set { _clientHost = value; }
		}

		[Property]
		public DateTime LogTime
		{
			get { return _logTime; }
			set { _logTime = value; }
		}

		[Property]
		public string UserName
		{
			get { return _userName; }
			set { _userName = value; }
		}

		[Property]
		public string TargetUserName
		{
			get { return _targetUserName; }
			set { _targetUserName = value; }
		}


		public static IList<PasswordChangeLogEntity> GetByLogin(string login, DateTime beginDate, DateTime endDate)
		{
			return ActiveRecordMediator<PasswordChangeLogEntity>
				.FindAll(new Order[] { Order.Asc("LogTime") },
						 Expression.Eq("TargetUserName", login)
						 && Expression.Between("LogTime", beginDate, endDate));
		}
	}
}
