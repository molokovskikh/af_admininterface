using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Logs;
using Castle.ActiveRecord;
using Common.Web.Ui.Helpers;
using NHibernate.Criterion;

namespace AdminInterface.Background
{
	public class UpdateAccountProcessor
	{
		public void Process()
		{
			foreach (var ids in Page<Account>(a => !a.ReadyForAccounting, 100))
			{
				using(var scope = new TransactionScope(OnDispose.Rollback))
				{
					foreach (var id in ids)
					{
						var account = Account.Find(id);
						if (account.ObjectType == LogObjectType.User)
						{
							var user = ((UserAccount)account).User;
							var updateCount = UpdateLogEntity.Queryable.Count(u => u.User == user
								&& u.Commit
								&& (u.UpdateType == UpdateType.Accumulative || u.UpdateType == UpdateType.Cumulative));
							if (updateCount >= 10)
							{
								account.ReadyForAccounting = true;
								account.Save();
							}
						}
						else if (account.ObjectType == LogObjectType.Address)
						{
							var address = ((AddressAccount)account).Address;
							if (address.AvaliableForUsers.Any(u => u.Accounting.ReadyForAccounting))
							{
								account.ReadyForAccounting = true;
								account.Save();
							}
						}
					}
					scope.VoteCommit();
				}
			}
		}

		public IEnumerable<uint[]> Page<T>(Expression<Func<T, bool>> expression, int size) where T : class 
		{
			int total;
			using (new SessionScope())
			{
				total = ArHelper.WithSession(s => {
					return s.QueryOver<T>()
						.Where(expression)
						.ToRowCountQuery()
						.FutureValue<int>()
						.Value;
				});
			}

			var pages = total / size + (total % size == 0 ? 0 : 1);
			for (var page = 0; page <= pages; page++)
			{
				uint[] result;
				using (new SessionScope())
				{
					result = ArHelper.WithSession(s => {
						return s.QueryOver<T>().Where(expression).Select(Projections.Id()).Skip(page*size).Take(size)
							.List<uint>()
							.ToArray();
					});
				}
				yield return result;
			}
		}
	}
}