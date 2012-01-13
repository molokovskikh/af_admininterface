using System;
using Castle.Components.Validator;
using Common.Web.Ui.Models.Validators;

namespace AdminInterface.Models.Validators
{
	public class ExtensionListValidationAttribute : AbstractValidationAttribute
	{
		private readonly IValidator validator;

		public ExtensionListValidationAttribute()
		{
			validator = new ExtensionListValidator();
		}

		public ExtensionListValidationAttribute(String errorMessage)
			: base(errorMessage)
		{
			validator = new ExtensionListValidator();
		}

		public override IValidator Build()
		{
			ConfigureValidatorMessage(validator);

			return validator;
		}
	}
}