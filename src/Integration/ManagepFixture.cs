using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using AddUser;
using AdminInterface.Models;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework.Config;
using NHibernate.Linq;
using NUnit.Framework;
using Test.Support;
using Test.Support.Suppliers;


namespace Integration
{
	public class ManagepFixture : Test.Support.IntegrationFixture
	{
		private TestSupplier _supplier;
		[SetUp]
		public void SetUp()
		{
			_supplier = TestSupplier.Create();
		}
		[Test]
		public void NewSpecialOrderHandlerTest()
		{
			var handler = session.Query<TestHandler>().First(t => t.ClassName.Contains("Formater") && !t.ClassName.Contains("Default"));
			var supplier = session.Query<Supplier>().First(t => t.Id == _supplier.Id);
			var managep = new managep();
			managep.DbSession = ActiveRecordMediator.GetSessionFactoryHolder().CreateSession(typeof(ActiveRecordBase));
			managep.CreateNewSpecialOrders(supplier, handler.Id, "Специальный формат", HandlerTypes.Formatter);
			var specHandler = session.Query<SpecialHandler>().First(t => t.Supplier == supplier && t.Handler.Id == handler.Id);
			Assert.That(specHandler.Name, Is.EqualTo("Специальный формат"));

			var specCount = session.Query<SpecialHandler>().Count();
			var defHandler = session.Query<OrderHandler>().First(t => t.ClassName == "DefaultXmlFormater");
			managep.CreateNewSpecialOrders(supplier, defHandler.Id, "Специальный формат", HandlerTypes.Formatter);
			Assert.That(specCount, Is.EqualTo(session.Query<SpecialHandler>().Count()));
		}

		[Test]
		public void NotCreateSpecialHandlerForDefaultSender()
		{
			var handler = session.Query<TestHandler>().First(t => t.ClassName.Contains("EmailSender"));
			var supplier = session.Query<Supplier>().First(t => t.Id == _supplier.Id);
			var managep = new managep();
			managep.DbSession = ActiveRecordMediator.GetSessionFactoryHolder().CreateSession(typeof(ActiveRecordBase));
			managep.CreateNewSpecialOrders(supplier, handler.Id, "Специальная доставка", HandlerTypes.Sender);
			var specHandler = session.Query<SpecialHandler>().FirstOrDefault(t => t.Supplier == supplier && t.Handler.Id == handler.Id);
			Assert.That(specHandler, Is.Null);
		}
	}
}
