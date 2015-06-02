using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdminInterface.Models;
using Castle.ActiveRecord;
using Castle.Components.Validator;
using Castle.MonoRail.Framework.Helpers;
using Common.MySql;
using NHibernate;

namespace AdminInterface.Models.Validators
{
	public class NameExistsValidator : AbstractValidator
	{
		public NameExistsValidator()
		{
			RunWhen = RunWhen.Insert | RunWhen.Update;
		}

		public override bool IsValid(object instance, object fieldValue)
		{
			var isValid = true;
			var sessionHolder = ActiveRecordMediator.GetSessionFactoryHolder();
			var session = sessionHolder.CreateSession(typeof(ActiveRecordBase));
			session.FlushMode = FlushMode.Never;
			var clientNameExists = session.QueryOver<Client>().Where(c => c.HomeRegion.Id == ((Client)instance).HomeRegion.Id && c.Name == fieldValue.ToString() && c.Id != ((Client)instance).Id).RowCount() > 0;
			var nameChanged = session.QueryOver<Client>().Where(c => c.Id == ((Client)instance).Id && c.Name == fieldValue).RowCount() == 0;
			if (clientNameExists && nameChanged) {
				isValid = false;
			}
			if (session != null) {
				sessionHolder.ReleaseSession(session);
			}
			return isValid;
		}

		public override bool SupportsBrowserValidation
		{
			get { return false; }
		}
	}

	public class NameExistsValidatorAttribute : AbstractValidationAttribute
	{
		private readonly IValidator validator;

		public NameExistsValidatorAttribute()
		{
			RunWhen = RunWhen.Insert | RunWhen.Update;
			validator = new NameExistsValidator();
		}

		public NameExistsValidatorAttribute(String errorMessage)
			: base(errorMessage)
		{
			RunWhen = RunWhen.Insert | RunWhen.Update;
			validator = new NameExistsValidator();
		}

		public override IValidator Build()
		{
			ConfigureValidatorMessage(validator);
			return validator;
		}
	}
}