using AdminInterface.Models.Billing;
using Castle.ActiveRecord;
using Castle.Components.Validator;
using Integration.ForTesting;
using NUnit.Framework;

namespace Integration.MonoRailExtentions
{
	[TestFixture]
	public class ValidatorToNHibernateIntegrationFixture : IntegrationFixture
	{
		[Test]
		public void Do_not_update_not_valid_entity()
		{
			var validator = new ValidatorRunner(new CachedValidationRegistry());
			var accessor = new StaticValidatorAccessor();
			accessor.Validator = validator;
			ValidEventListner.ValidatorAccessor = accessor;

			var inn = new IgnoredInn("Test");
			inn.Save();
			Assert.That(inn.Id, Is.Not.EqualTo(0));
			inn.Name = null;
			Assert.That(validator.IsValid(inn), Is.False);
			scope.Flush();
			scope.Dispose();
			scope = new TransactionlessSession();

			inn = IgnoredInn.Find(inn.Id);
			Assert.That(inn.Name, Is.EqualTo("Test"));
		}

		[Test]
		public void Test()
		{
			var validator = new ValidatorRunner(new CachedValidationRegistry());
			var inn = new Nomenclature();
			Assert.That(validator.IsValid(inn), Is.False);
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