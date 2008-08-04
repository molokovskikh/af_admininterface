//using System;
//using System.Collections.Generic;
//using NHibernate;
//using NHibernate.Criterion;
//using NHibernate.SqlCommand;
//using NHibernate.Type;
//using NHibernate.Util;

//namespace AdminInterface.Models
//{
//    public class BitOrProjection : SimpleProjection
//    {
//        protected readonly IProjection left;
//        protected readonly IProjection right;

//        public BitOrProjection(IProjection left, IProjection right)
//        {
//            this.right = right;
//            this.left = left;
//        }

//        public override bool IsAggregate
//        {
//            get { return false; }
//        }

//        public override bool IsGrouped
//        {
//            get { return false; }
//        }

//        public override IType[] GetTypes(ICriteria criteria, ICriteriaQuery criteriaQuery)
//        {
//            return new IType[] { NHibernateUtil.Int32 };
//        }

//        public override SqlString ToSqlString(ICriteria criteria, int loc, ICriteriaQuery criteriaQuery, IDictionary<string, IFilter> enabledFilters)
//        {
//            return new SqlString()
//                .Append(StringHelper.RemoveAsAliasesFromSql(left.ToSqlString(criteria, loc, criteriaQuery, enabledFilters)))
//                .Append(" & ")
//                .Append(StringHelper.RemoveAsAliasesFromSql(right.ToSqlString(criteria, loc, criteriaQuery, enabledFilters)))
//                .Append(" as y")
//                .Append(loc.ToString())
//                .Append("_");

//        }

//        public override SqlString ToGroupSqlString(ICriteria criteria, ICriteriaQuery criteriaQuery,
//                                                   IDictionary<string, IFilter> enabledFilters)
//        {
//            throw new InvalidOperationException("BitOrProjection not a grouping projection");
//        }
//    }
//}