using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdminInterface.Models.Billing;
using Castle.Components.Validator;

namespace AdminInterface.Models.Validators
{
	public class PayerNotNullValidator : AbstractValidator
	{
		public override bool IsValid(object instance, object fieldValue)
		{
			var payer = fieldValue as Payer;
			if(payer != null && payer.Id > 0)
				return true;
			return false;
		}

		public override bool SupportsBrowserValidation
		{
			get { return false; }
		}
	}

	public class PayerNotNullValidatorAttribute : AbstractValidationAttribute
	{
		private readonly IValidator validator;

		public PayerNotNullValidatorAttribute()
		{
			validator = new PayerNotNullValidator();
		}

		public PayerNotNullValidatorAttribute(String errorMessage)
			: base(errorMessage)
		{
			validator = new PayerNotNullValidator();
		}

		public override IValidator Build()
		{
			ConfigureValidatorMessage(validator);
			return validator;
		}
	}
}