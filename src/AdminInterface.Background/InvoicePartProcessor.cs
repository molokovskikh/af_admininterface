using System;
using System.Linq;
using AdminInterface.Models.Billing;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;

namespace AdminInterface.Background
{
	public class InvoicePartProcessor
	{
		public void Process()
		{
			uint[] forProcessing;
			using(new SessionScope())
				forProcessing = ActiveRecordLinqBase<InvoicePart>
					.Queryable
					.Where(p => p.PayDate <= DateTime.Today && p.Processed == false)
					.Select(p => p.Id)
					.ToArray();

			using (var scope = new TransactionScope(OnDispose.Rollback))
			{
				foreach (var id in forProcessing)
				{
					var part = ActiveRecordMediator<InvoicePart>.FindByPrimaryKey(id);
					part.Process();
					ActiveRecordMediator.Save(part.Invoice);
					ActiveRecordMediator.Save(part);
				}

				scope.Flush();
				scope.VoteCommit();
			}
		}
	}
}