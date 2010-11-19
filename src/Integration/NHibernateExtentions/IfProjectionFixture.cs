using AdminInterface.Models;
using AdminInterface.NHibernateExtentions;
using Castle.ActiveRecord;
using Functional.ForTesting;
using NHibernate.Criterion;
using NUnit.Framework;

namespace Integration.NHibernateExtentions
{
	[TestFixture]
	public class IfProjectionFixture
	{
		public IfProjectionFixture()
		{
			ForTest.InitialzeAR();
		}

		[Test]
		public void IfProjectionInHaving()
		{
			//Projections.Conditional()
			ActiveRecordMediator<Payer>.Execute(
				(session, target) =>
					{
						var s = session.CreateCriteria(typeof (Payer))
							.CreateAlias("Clients", "cd")
							.SetProjection(Projections.ProjectionList()
							               	.Add(Projections.Id())
							               	.Add(Projections.Property("ShortName"))
							               	.Add(Projections.GroupProperty("PayerID")))
							.Add(Restrictions.Ge(new AggregateProjection2("sum",
							                                            Projections.Conditional(Expression.Eq("cd.Status", ClientStatus.Off),
							                                                                    Projections.Constant(1),
							                                                                    Projections.Constant(0))),
							                   10))
							.List();
						return null;
					}, 
				null);
		}

		[Test]
		public void IfProjectionInSelectBlock()
		{
			ActiveRecordMediator<Payer>.Execute(
				(session, target) =>
					{
						var s = session.CreateCriteria(typeof (Payer))
							.CreateAlias("Clients", "cd")
							.SetProjection(Projections.ProjectionList()
							               	.Add(Projections.Id())
							               	.Add(Projections.Property("ShortName"))
							               	.Add(Projections.GroupProperty("PayerID"))
							               	.Add(new AggregateProjection2("sum",
							               	                              Projections.Conditional(
							               	                              	Expression.Eq("cd.Status", ClientStatus.Off),
							               	                              	Projections.Constant(1),
							               	                              	Projections.Constant(0))))
							)
							.List();
						return null;
					},
				null);
		}
	}
}
