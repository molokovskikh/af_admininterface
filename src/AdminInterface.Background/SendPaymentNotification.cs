using System.Linq;
using AdminInterface.Models;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.ActiveRecord.Linq;

namespace AdminInterface.Background
{
	public class SendPaymentNotification
	{
		public void Process()
		{
			using(new SessionScope())
			{
				var payers = ActiveRecordLinq.AsQueryable<Payer>()
					.Where(p => p.SendPaymentNotification && p.Balance < 0)
					.ToList();
				foreach (var payer in payers)
				{
					//payer.se
				}
			}
		}
	}
}