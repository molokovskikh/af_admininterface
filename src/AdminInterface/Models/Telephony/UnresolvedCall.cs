using System.Linq;
using AdminInterface.Models.Logs;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Common.Web.Ui.ActiveRecordExtentions;
using Common.Web.Ui.Helpers;
using NHibernate.Criterion;

namespace AdminInterface.Models.Telephony
{
	[ActiveRecord("UnresolvedPhone", Schema = "telephony")]
	public class UnresolvedCall
	{
		public UnresolvedCall()
		{}

		public UnresolvedCall(string phone)
		{
			PhoneNumber = phone;
		}

		[PrimaryKey("id")]
		public virtual ulong Id { get; set; }

		[Property("Phone")]
		public virtual string PhoneNumber { get; set; }

		public static string[] LastCalls
		{
			get
			{
				var criteria = DetachedCriteria.For<UnresolvedCall>()
					.SetProjection(Projections.Group<UnresolvedCall>(l => l.PhoneNumber))
					.AddOrder(Order.Desc("Id"))
					.SetMaxResults(5);

				return ArHelper.WithSession(s => criteria.GetExecutableCriteria(s).List<string>().ToArray());

/*
				return (from call in Queryable
					orderby call.Id descending
					group call by call.PhoneNumber into c
					select c.Key).Take(5).ToArray();
*/
			}
		}
	}
}
