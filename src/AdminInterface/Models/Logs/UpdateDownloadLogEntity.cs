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

namespace AdminInterface.Model
{
	[ActiveRecord(Table = "logs.AnalitFUpdates")]
	public class UpdateDownloadLogEntity : ActiveRecordBase<UpdateDownloadLogEntity>
	{
		private uint _id;
		private string _log;

		[PrimaryKey("UpdateId")]
		public uint Id
		{
			get { return _id; }
			set { _id = value; }
		}

		[Property]
		public string Log
		{
			get { return _log; }
			set { _log = value; }
		}
	}
}
