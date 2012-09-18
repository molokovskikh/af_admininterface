﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdminInterface.Models;
using Castle.ActiveRecord;
using Castle.Components.Validator;
using Common.MySql;

namespace AdminInterface.Models.Validators
{
	public class NameExistsValidator : AbstractValidator
	{
		public override bool IsValid(object instance, object fieldValue)
		{
			var sessionHolder = ActiveRecordMediator.GetSessionFactoryHolder();
			var DbSession = sessionHolder.CreateSession(typeof(ActiveRecordBase));
			var clientNameExists = DbSession.QueryOver<Client>().Where(c => c.HomeRegion.Id == ((Client)instance).HomeRegion.Id && c.Name == fieldValue.ToString() && c.Id != ((Client)instance).Id).RowCount() > 0;
			if (clientNameExists) {
				return false;
			}
			if (DbSession != null) {
				sessionHolder = ActiveRecordMediator.GetSessionFactoryHolder();
				sessionHolder.ReleaseSession(DbSession);
			}
			return true;
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
			validator = new NameExistsValidator();
		}

		public NameExistsValidatorAttribute(String errorMessage)
			: base(errorMessage)
		{
			validator = new NameExistsValidator();
		}

		public override IValidator Build()
		{
			ConfigureValidatorMessage(validator);
			return validator;
		}
	}
}