using AdminInterface.Models;
using Castle.ActiveRecord;
using Integration.ForTesting;
using NHibernate.Criterion;
using NUnit.Framework;

namespace Integration.NHibernateExtentions
{
	[TestFixture]
	public class BitOrProjectionTest 
	{
		[SetUp]
		public void SetUp()
		{
			ForTest.InitialzeAR();
		}

		[Test]
		public void BitOrInWhereTest()
		{
			ActiveRecordMediator<Client>.Execute(
				(session, target) =>
				{
					var s = session.CreateCriteria(typeof(Client))
						.Add(Expression.Ge(Projections2.BitOr("MaskRegion", 1), 0))
						.List();
					return null;
				},
				null);
		}

		[Test]
		public void BitOrInSelectTest()
		{
			ActiveRecordMediator<Client>.Execute(
				(session, target) =>
				{
					var s = session.CreateCriteria(typeof (Client))
						.SetProjection(Projections2.BitOr("MaskRegion", 1))
						.Add(Expression.Ge(Projections2.BitOr("MaskRegion", 1), 0))
						.List();
					return null;
				},
				null);
		}

	}
}
