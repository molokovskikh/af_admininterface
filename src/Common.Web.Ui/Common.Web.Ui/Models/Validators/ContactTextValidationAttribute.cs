using Castle.Components.Validator;

namespace Common.Web.Ui.Models.Validators
{
	public class ContactTextValidationAttribute : AbstractValidationAttribute
	{
		public ContactTextValidationAttribute(string errorMessage) : base(errorMessage)
		{}

		public override IValidator Build()
		{
			IValidator validator = new ContactTextValidator();

			ConfigureValidatorMessage(validator);

			return validator;
		}
	}
}
