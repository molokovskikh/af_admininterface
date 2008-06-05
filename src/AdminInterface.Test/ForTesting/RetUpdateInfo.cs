using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.ActiveRecord;
using Common.Web.Ui.Models;

namespace AdminInterface.Test.ForTesting
{
	[ActiveRecord("usersettings.ret_update_info")]
	public class RetUpdateInfo
	{
		[PrimaryKey("ClientCode")]
		public uint Id { get; set; }

		[Property]
		public string UniqueCopyID { get; set; }

		public void Save()
		{
			ActiveRecordMediator<RetUpdateInfo>.Execute(
				(session, instance) =>
					{
						session.Save(this, this.Id);
						return null;
					},
				null);
		}

		public static RetUpdateInfo Get(uint id)
		{
			return ActiveRecordMediator<RetUpdateInfo>.FindByPrimaryKey(id);
		}
	}
}
