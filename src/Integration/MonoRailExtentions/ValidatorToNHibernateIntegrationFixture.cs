using AdminInterface.Models.Billing;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;
using Castle.Components.Validator;
using Common.Web.Ui.MonoRailExtentions;
using Integration.ForTesting;
using NHibernate;
using NHibernate.Proxy;
using NUnit.Framework;
using Test.Support;

namespace Integration.MonoRailExtentions
{
	[TestFixture]
	public class ValidatorToNHibernateIntegrationFixture : AdmIntegrationFixture
	{
		private StaticValidatorAccessor accessor;
		private ValidatorRunner validator;

		[SetUp]
		public void Setup()
		{
			validator = new ValidatorRunner(new CachedValidationRegistry());
			accessor = new StaticValidatorAccessor();
			accessor.Validator = validator;
			ValidEventListner.ValidatorAccessor = accessor;
		}

		[TearDown]
		public void TearDown()
		{
			ValidEventListner.ValidatorAccessor = null;
		}

		[Test]
		public void Do_not_update_not_valid_entity()
		{
			var inn = new IgnoredInn("Test");
			session.Save(inn);
			Assert.That(inn.Id, Is.Not.EqualTo(0));
			inn.Name = null;
			Assert.That(validator.IsValid(inn), Is.False);

			Reopen();
			inn = IgnoredInn.Find(inn.Id);
			Assert.That(inn.Name, Is.EqualTo("Test"));
		}

		[Test]
		public void Check_proxy_for_validation_error()
		{
			var supplier = DataMother.CreateSupplier();
			Save(supplier);

			session.Clear();
			var item = session.Load<Supplier>(supplier.Id);
			Assert.That(item as INHibernateProxy, Is.Not.Null);
			item.Name = "";
			Assert.That(validator.IsValid(item), Is.False);

			session.Clear();
			item = session.Load<Supplier>(supplier.Id);
			Assert.That(item.Name, Is.EqualTo("Тестовый поставщик"));
		}
	}

	public class StaticValidatorAccessor : IValidatorAccessor
	{
		private static ValidatorRunner _validator;

		public ValidatorRunner Validator
		{
			get { return _validator; }
			set { _validator = value; }
		}
	}
}