using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Type;
using NHibernate.Util;

namespace AdminInterface.NHibernateExtentions
{
	[Serializable]
	public class AggregateProjection2 : SimpleProjection
	{
		protected readonly IProjection propertyName;
		protected readonly string aggregate;

		public AggregateProjection2(string aggregate, IProjection projection)
		{
			this.aggregate = aggregate;
			this.propertyName = projection;
		}

		public override bool IsAggregate
		{
			get { return true; }
		}

		public override string ToString()
		{
			return aggregate + "(" + propertyName + ')';
		}

		public override IType[] GetTypes(ICriteria criteria, ICriteriaQuery criteriaQuery)
		{
			return propertyName.GetTypes(criteria, criteriaQuery);
		}

		public override SqlString ToSqlString(ICriteria criteria, int loc, ICriteriaQuery criteriaQuery, IDictionary<string, IFilter> enabledFilters)
		{
			return new SqlString()
				.Append(aggregate)
				.Append("(")
				.Append(StringHelper.RemoveAsAliasesFromSql(propertyName.ToSqlString(criteria, loc, criteriaQuery, enabledFilters)))
				.Append(")")
				.Append(" as y")
				.Append(loc.ToString())
				.Append("_");

		}

		public override bool IsGrouped
		{
			get { return false; }
		}

		public override SqlString ToGroupSqlString(ICriteria criteria, ICriteriaQuery criteriaQuery,
												   IDictionary<string, IFilter> enabledFilters)
		{
			throw new InvalidOperationException("not a grouping projection");
		}
	}
}