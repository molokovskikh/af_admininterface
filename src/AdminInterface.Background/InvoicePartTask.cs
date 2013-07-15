using System;
using System.Linq;
using AdminInterface.Models.Billing;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Common.Web.Ui.Helpers;
using NHibernate;

namespace AdminInterface.Background
{
	public class InvoicePartTask : Task
	{
		protected override void Process()
		{
			var ids = ActiveRecordLinqBase<InvoicePart>
				.Queryable
				.Where(p => p.PayDate <= DateTime.Today && p.Processed == false)
				.Select(p => p.Id)
				.ToArray();

			foreach (var id in ids) {
				var part = ActiveRecordMediator<InvoicePart>.FindByPrimaryKey(id);
				part.Process();
				ActiveRecordMediator.Save(part.Invoice);
				ActiveRecordMediator.Save(part);
			}
		}
	}
}