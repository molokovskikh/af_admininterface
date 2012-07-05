using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Components.Validator;
using System.Text.RegularExpressions;

namespace AdminInterface.Models.Validators
{
	public class ValidateForbiddenSymbols : AbstractValidationAttribute
	{
		public const string RegexTpl = @"^[\wа-яА-Я-Ёё]+$";
		private readonly string _errorMessage;

		public ValidateForbiddenSymbols()
		{
			_errorMessage = "Поле может содержать только буквы, цифры и знаки('_', '-')";
		}

		public ValidateForbiddenSymbols(string errorMessage) : base(errorMessage)
		{
			_errorMessage = errorMessage;
		}

		public override IValidator Build()
		{
			var validator = new RegularExpressionValidator(RegexTpl);
			validator.ErrorMessage = _errorMessage;
			ConfigureValidatorMessage(validator);
			return validator;
		}
	}
}
