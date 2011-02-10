﻿using System;
using System.Collections;
using AdminInterface.Controllers;
using Castle.ActiveRecord;
using Castle.Components.Validator;
using Common.Web.Ui.Helpers;

namespace AdminInterface.Models.Billing
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
			if (fieldValue != null && !(fieldValue.ToString() == ""))
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

	[ActiveRecord(Schema = "billing")]
	public class InvoicePart
	{
		[PrimaryKey]
		public uint Id { get; set; }

		[Property, ValidateNonEmpty]
		public string Name { get; set; }

		[Property, ValidateGreaterThanZero]
		public decimal Cost { get; set; }

		[Property, ValidateGreaterThanZero]
		public int Count { get; set; }

		[BelongsTo]
		public Invoice Invoice { get; set; }

		public decimal Sum
		{
			get
			{
				return Cost * Count;
			}
		}

		public InvoicePart()
		{}

		public InvoicePart(Invoice invoice, Period period, decimal cost, int count)
		{
			Invoice = invoice;
			if (invoice.Recipient.Id == 4)
				Name = String.Format("Обеспечение доступа к ИС (мониторингу фармрынка) в {0}", GetPeriodName(period));
			else
				Name = String.Format("Мониторинг оптового фармрынка за {0}", BindingHelper.GetDescription(period).ToLower());
			Cost = cost;
			Count = count;
		}

		public string GetPeriodName(Period period)
		{
			switch (period)
			{
				case Period.January:
					return "январе";
				case Period.February:
					return "феврале";
				case Period.March:
					return "марте";
				case Period.April:
					return "апреле";
				case Period.August:
					return "августе";
				case Period.December:
					return "декабре";
				case Period.July:
					return "июле";
				case Period.June:
					return "июне";
				case Period.May:
					return "мае";
				case Period.November:
					return "ноябре";
				case Period.October:
					return "октябре";
				case Period.September:
					return "сентябре";
				case Period.FirstQuarter:
					return "первом квартале";
				case Period.SecondQuarter:
					return "втором квартале";
				case Period.ThirdQuarter:
					return "третьем квартале";
				case Period.FourthQuarter:
					return "четвертом квартале";
				default:
					throw new Exception(String.Format("не знаю что за период такой {0}", period));
			}
		}
	}
}