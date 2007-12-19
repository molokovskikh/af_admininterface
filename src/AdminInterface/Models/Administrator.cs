using System;
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

namespace AdminInterface.Models
{
	[ActiveRecord("accessright.regionaladmins")]
	public class Administrator
	{
		private bool _alowChangePassword;
		private uint _id;
		private string _userName;

		[PrimaryKey("RowId")]
		public uint Id
		{
			get { return _id; }
			set { _id = value; }
		}

		[Property]
		public virtual bool AlowChangePassword
		{
			get { return _alowChangePassword; }
			set { _alowChangePassword = value; }
		}

		[Property]
		public virtual string UserName
		{
			get { return _userName; }
			set { _userName = value; }
		}

		public static Administrator GetByName(string name)
		{
			return ActiveRecordMediator<Administrator>.FindOne(Expression.Eq("UserName", name.Replace("ANALIT\\", "")));
		}
	}
}
