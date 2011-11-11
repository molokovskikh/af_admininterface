using System.Collections;
using Castle.Components.Validator;

namespace AdminInterface.MonoRailExtentions
{
	public class GreaterThanZeroValidator : AbstractValidator
	{
		public GreaterThanZeroValidator()
		{
			ErrorMessage = "Значение должно быть больше нуля";
		}

		public override void ApplyBrowserValidation(BrowserValidationConfiguration config,
			InputElementType inputType,
			IBrowserValidationGenerator generator,
			IDictionary attributes,
			string target)
		{
			base.ApplyBrowserValidation(config, inputType, generator, attributes, target);
			generator.SetDigitsOnly(target, BuildErrorMessage());
		}

		public override bool IsValid(object instance, object fieldValue)
		{
			if (fieldValue != null && fieldValue.ToString() != "")
			{
				decimal num;
				if (!decimal.TryParse(fieldValue.ToString(), out num))
					return false;

				return num > 0;
			}
			return true;
		}

		public override bool SupportsBrowserValidation
		{
			get
			{
				return true;
			}
		}
	}

	public class ValidateGreaterThanZero : AbstractValidationAttribute
	{
		public override IValidator Build()
		{
			var validator = new GreaterThanZeroValidator();
			ConfigureValidatorMessage(validator);
			return validator;
		}
	}
}
