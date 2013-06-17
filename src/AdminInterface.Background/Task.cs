using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Castle.ActiveRecord;
using Common.Web.Ui.ActiveRecordExtentions;
using NHibernate;
using NHibernate.Criterion;
using Expression = NHibernate.Criterion.Expression;

namespace AdminInterface.Background
{
	public abstract class Task
	{
		protected ISession Session;

		public void Execute()
		{
			using (var scope = new SessionScope()) {
				ArHelper.WithSession(s => {
					Session = s;
					try {
						Process();
						Session.Flush();
					}
					catch(Exception) {
						scope.HasSessionError = true;
					}
					finally {
						Session = null;
					}
				});
			}
		}

		protected abstract void Process();

		protected IEnumerable<uint[]> Page<T>(Expression<Func<T, bool>> expression, int size) where T : class
		{
			var total = Session.QueryOver<T>()
				.Where(expression)
				.ToRowCountQuery()
				.FutureValue<int>()
				.Value;

			var pages = total / size + (total % size == 0 ? 0 : 1);
			uint lastId = 0;
			for (var page = 0; page <= pages; page++) {
				var result = Session.QueryOver<T>().Where(expression)
					.And(Expression.Gt(Projections.Id(), lastId))
					.Select(Projections.Id())
					.Take(size)
					.List<uint>()
					.ToArray();
				if(result.Length > 0)
					lastId = result.Max();
				yield return result;
			}
		}

		protected void PagedProcess(IEnumerable<uint[]> paged, Action<uint[]> action)
		{
			Session.Transaction.Commit();
			foreach (var ids in paged) {
				using (Session.BeginTransaction()) {
					Session.Clear();
					action(ids);
					Session.Flush();
					Session.Transaction.Commit();
				}
			}
		}
	}
}