//using AdminInterface.NHibernateExtentions;
//using NHibernate.Criterion;

//namespace AdminInterface.Models
//{
//    public static class Projections2
//    {
//        public static IProjection Sum(IProjection projection)
//        {
//            return new AggregateProjection2("sum", projection);
//        }

//        public static IProjection BitOr(string propertyName, object value)
//        {
//            return new BitOrProjection(Projections.Property(propertyName), Projections.Constant(value));
//        }
//    }
//}
