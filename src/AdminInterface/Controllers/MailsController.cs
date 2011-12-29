using System;
using System.Collections.Generic;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Documents;
using AdminInterface.Models.Suppliers;
using AdminInterface.MonoRailExtentions;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.MonoRail.ActiveRecordSupport;
using Common.Web.Ui.Helpers;

namespace AdminInterface.Controllers
{
	public class MailFilter
	{
		public Supplier Supplier { get; set; }
		public Client Client { get; set; }
		public User User { get; set; }
		public DatePeriod Period { get; set; }

		public MailFilter()
		{
			Period = new DatePeriod(DateTime.Today.AddDays(-14), DateTime.Today);
		}

		public IList<MailSendLog> Find()
		{
			return ActiveRecordLinqBase<MailSendLog>
				.Queryable
				.Where(l => l.User.Client == Client)
				.ToList();
		}
	}

	public class MailsController : AdminInterfaceController
	{
		public void Index([ARDataBind("filter", AutoLoad = AutoLoadBehavior.Always)] MailFilter filter)
		{
			PropertyBag["filter"] = filter;
			PropertyBag["logs"] = filter.Find();
		}

		public void Body(uint id)
		{
			CancelLayout();

			var log = ActiveRecordMediator<MailSendLog>.FindByPrimaryKey(id);
			PropertyBag["log"] = log;
		}

		public void Attachments(uint id)
		{
			CancelLayout();

			var log = ActiveRecordMediator<MailSendLog>.FindByPrimaryKey(id);
			PropertyBag["log"] = log;
		}

		public void Attachment(uint id)
		{
			var attachment = ActiveRecordMediator<Attachment>.FindByPrimaryKey(id);
			this.RenderFile(attachment.StorageFilename(Config), attachment.FullFilename);
		}
	}
}