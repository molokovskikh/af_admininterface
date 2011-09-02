using AdminInterface.Models.Billing;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;
using Castle.Components.Validator;
using Integration.ForTesting;
using NHibernate;
using NHibernate.Proxy;
using NUnit.Framework;

namespace Integration.MonoRailExtentions
{
	[TestFixture]
	public class ValidatorToNHibernateIntegrationFixture : IntegrationFixture
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

		[Test]
		public void Do_not_update_not_valid_entity()
		{
			var inn = new IgnoredInn("Test");
			inn.Save();
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
			uint id;
			using (new SessionScope())
			{
				var supplier = DataMother.CreateSupplier();
				supplier.Save();
				id = supplier.Id;
			}

			var item = Supplier.Find(id);
			Assert.That(item as INHibernateProxy, Is.Not.Null);
			item.Name = "";
			Assert.That(validator.IsValid(item), Is.False);

			Reopen();
			item = Supplier.Find(id);
			Assert.That(item.Name, Is.EqualTo("Тестовый поставщик"));
		}

		private void Reopen()
		{
			scope.Flush();
			scope.Dispose();
			scope = new TransactionlessSession();
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