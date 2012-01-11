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
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;

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
			return ArHelper.WithSession(s => {
				var query = s.QueryOver<MailSendLog>();
				query.JoinQueryOver(m => m.Update, JoinType.LeftOuterJoin);
				var mailJoin = query.JoinQueryOver(l => l.Mail, JoinType.InnerJoin);
				var user = query.JoinQueryOver(l => l.User, JoinType.InnerJoin);
				query.JoinQueryOver(l => l.Recipient, JoinType.InnerJoin);
				mailJoin.JoinQueryOver(m => m.Attachments, JoinType.LeftOuterJoin);

				if (Client != null)
					user.Where(u => u.Client == Client);

				if (User != null)
					query.Where(l => l.User == User);

				if (Supplier != null)
					mailJoin.Where(m => m.Supplier == Supplier);

				var dummy = mailJoin
					.Where(m => m.LogTime >= Period.Begin && m.LogTime < Period.End.AddDays(1))
					.OrderBy(m => m.LogTime).Desc;
				query.TransformUsing(Transformers.DistinctRootEntity);
				return query.List();
			});
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