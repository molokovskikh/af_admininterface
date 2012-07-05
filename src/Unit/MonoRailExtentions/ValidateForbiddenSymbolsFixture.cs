using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Components.Validator;
using Castle.MonoRail.Framework.Helpers.ValidationStrategy;
using Common.Web.Ui.MonoRailExtentions;
using Common.Web.Ui.MonoRailExtentions.Validation;
using NUnit.Framework;
using AdminInterface.Models.Validators;

namespace Unit.MonoRailExtentions
{
	[TestFixture]
	public class ValidateForbiddenSymbolsFixture
	{
		[Test]
		public void TestValueOnForbiddenSymbols()
		{
			var configuration = new JQueryValidator.JQueryConfiguration();
			var generator = new FixJQueryValidator.FixJQueryValidationGenerator(configuration);
			generator.SetRegExp("name", ValidateForbiddenSymbols.RegexTpl, "тест на запрещенные символы");
			var html = configuration.CreateBeforeFormClosed("id");
			Assert.That(html, Is.StringContaining(@"[\\wа-яА-Я-Ёё]+$"));
		}
	}
}
