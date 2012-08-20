using System;
using Castle.Components.Validator;

namespace AdminInterface.Models.Validators
{
	public class ExtensionListValidator : AbstractValidator
	{
		public override bool IsValid(object instance, object fieldValue)
		{
			if (fieldValue == null)
				return true;

			var extensionList = fieldValue.ToString().Split(',');
			foreach (var s in extensionList) {
				var trimedValue = s.Trim();
				foreach (var testedSymbol in trimedValue) {
					if (!char.IsLetterOrDigit(testedSymbol))
						return false;
				}
			}

			return true;
		}

		public override bool SupportsBrowserValidation
		{
			get { return false; }
		}

		protected override string BuildErrorMessage()
		{
			if (!String.IsNullOrEmpty(ErrorMessage))
				return ErrorMessage;
			return "Список расширений должен быть как: doc, tif, jpg";
		}
	}
}