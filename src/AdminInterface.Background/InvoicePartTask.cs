﻿using System;
using System.Linq;
using AdminInterface.Models.Billing;
using Common.Web.Ui.Helpers;
using NHibernate;
using NHibernate.Linq;

namespace AdminInterface.Background
{
	public class InvoicePartTask : Task
	{
		public InvoicePartTask()
		{
		}

		public InvoicePartTask(ISession session) : base(session)
		{
		}

		protected override void Process()
		{
			var ids = Session.Query<InvoicePart>()
				.Where(p => p.PayDate <= DateTime.Today && p.Processed == false)
				.Select(p => p.Id)
				.ToArray();

			foreach (var id in ids) {
				var part = Session.Load<InvoicePart>(id);
				part.Process();
				Session.Save(part.Invoice);
				Session.Save(part);
			}
		}
	}
}