using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Castle.ActiveRecord;
using Common.Tools;
using Common.Web.Ui.Models.Audit;

namespace AdminInterface.Models
{
	public enum MarkupType
	{
		Over,
		VitallyImportant,
		Nds18,
		Special
	}

	[ActiveRecord("MarkupGlobalConfig", Schema = "usersettings"), Auditable]
	public class MarkupGlobalConfig
	{
		private decimal begin;
		private bool beginOverlap;
		private decimal end;
		private bool endLessThanBegin;
		private bool haveGap;

		public MarkupGlobalConfig()
		{
		}

		public MarkupGlobalConfig(
			Client client,
			MarkupType type = MarkupType.Over)
		{
			Type = type;
			Client = client;
		}

		public MarkupGlobalConfig(decimal begin,
			decimal end,
			decimal markup,
			Client client,
			MarkupType type = MarkupType.Over)
		{
			Begin = begin;
			End = end;
			Markup = markup;
			MaxMarkup = markup;
			Type = type;
			Client = client;
		}

		[PrimaryKey]
		public virtual uint Id { get; set; }


		[BelongsTo(Column = "ClientId")]
		public virtual Client Client { get; set; }

		[Display(Name = "Тип")]
		[Property]
		public virtual MarkupType Type { get; set; }

		[Display(Name = "Левая граница цен")]
		[Property]
		public virtual decimal Begin { get; set; }

		[Display(Name = "Правая граница цен")]
		[Property]
		public virtual decimal End { get; set; }

		[Display(Name = "Наценка (%)")]
		[Property]
		public virtual decimal Markup { get; set; }

		[Display(Name = "Макс. наценка (%)")]
		[Property]
		public virtual decimal MaxMarkup { get; set; }

		[Display(Name = "Макс. наценка опт. звена (%)")]
		[Property]
		public virtual decimal MaxSupplierMarkup { get; set; }


		public virtual string ViewBegin
		{
			get { return Math.Round(Begin, 2).ToString(); }
			set
			{
				var begin = 0m;
				decimal.TryParse(value?.Replace(".", ","), out begin);
				Begin = begin;
			}
		}

		public virtual string ViewEnd
		{
			get { return Math.Round(End, 2).ToString(); }
			set
			{
				var end = 0m;
				decimal.TryParse(value?.Replace(".", ","), out end);
				End = end;
			}
		}

		public virtual string ViewMarkup
		{
			get { return Math.Round(Markup, 2).ToString(); }
			set
			{
				var tempValue = 0m;
				decimal.TryParse(value?.Replace(".", ","), out tempValue);
				Markup = tempValue;
			}
		}

		public virtual string ViewMaxMarkup
		{
			get { return Math.Round(MaxMarkup, 2).ToString(); }
			set
			{
				var tempValue = 0m;
				decimal.TryParse(value?.Replace(".", ","), out tempValue);
				MaxMarkup = tempValue;
			}
		}

		public virtual string ViewMaxSupplierMarkup
		{
			get { return Math.Round(MaxSupplierMarkup, 2).ToString(); }
			set
			{
				var tempValue = 0m;
				decimal.TryParse(value?.Replace(".", ","), out tempValue);
				MaxSupplierMarkup = tempValue;
			}
		}


		public virtual bool BeginOverlap
		{
			get { return beginOverlap; }
			set
			{
				if (beginOverlap == value)
					return;

				beginOverlap = value;
			}
		}

		public virtual bool HaveGap
		{
			get { return haveGap; }
			set
			{
				if (haveGap == value)
					return;

				haveGap = value;
			}
		}

		public virtual bool EndLessThanBegin
		{
			get { return endLessThanBegin; }
			set
			{
				if (endLessThanBegin == value)
					return;
				endLessThanBegin = value;
			}
		}

		public static string GetTypeDescription(MarkupType type)
		{
			if (type == MarkupType.VitallyImportant)
				return "Наценки ЖНВЛС";
			if (type == MarkupType.Over)
				return "Наценки на прочий ассортимент";
			if (type == MarkupType.Nds18)
				return "Наценки товара с 18% НДС";
			if (type == MarkupType.Special)
				return "Специальные наценки";
			return "Не определен";
		}

		public static List<MarkupGlobalConfig> Defaults(Client client)
		{
			return new List<MarkupGlobalConfig> {
				new MarkupGlobalConfig(0, 1000000, 20, client),
				new MarkupGlobalConfig(0, 1000000, 20, client, MarkupType.Nds18),
				new MarkupGlobalConfig(0, 50, 20, client, MarkupType.VitallyImportant),
				new MarkupGlobalConfig(50, 500, 20, client, MarkupType.VitallyImportant),
				new MarkupGlobalConfig(500, 1000000, 20, client, MarkupType.VitallyImportant),
				new MarkupGlobalConfig(0, 1000000, 20, client, MarkupType.Special)
			}.OrderBy(s => s.Type).ThenBy(s => s.Begin).ToList();
		}

		public static List<string[]> Validate(IEnumerable<MarkupGlobalConfig> source)
		{
			var groups = source.GroupBy(c => new {c.Type});
			var errors = new List<string[]>();
			foreach (var markups in groups) {
				var data = markups.OrderBy(m => m.Begin).ToArray();
				markups.Each(x => {
					x.HaveGap = false;
					x.EndLessThanBegin = false;
					x.BeginOverlap = false;
				});
				foreach (var markup in data) {
					markup.EndLessThanBegin = markup.End < markup.Begin;
				}

				var prev = data.First();
				foreach (var markup in data.Skip(1)) {
					markup.BeginOverlap = prev.End > markup.Begin;
					markup.HaveGap = prev.End < markup.Begin;
					if (markup.Markup > markup.MaxMarkup) {
						errors.Add(new[] {"Максимальная наценка меньше наценки."});
					}

					if (markup.BeginOverlap || markup.EndLessThanBegin || markup.HaveGap) {
						var errorMessage =
							markup.BeginOverlap
								? $"левая граница цен попадает в диапазон {Math.Round(prev.Begin, 2).ToString("C")} - {Math.Round(prev.End, 2).ToString("C")}"
								: markup.EndLessThanBegin
									? $"интервалы пересекаются друг с другом. Ошибка для интервала {Math.Round(prev.Begin, 2).ToString("C")} - {Math.Round(prev.End, 2).ToString("C")} и {Math.Round(markup.Begin, 2).ToString("C")} - {Math.Round(markup.End, 2).ToString("C")}"
									: markup.HaveGap
										? $"имеется разрыв между интервалами {Math.Round(prev.Begin, 2).ToString("C")} - {Math.Round(prev.End, 2).ToString("C")} и {Math.Round(markup.Begin, 2).ToString("C")} - {Math.Round(markup.End, 2).ToString("C")}"
										: "";
						errors.Add(new[] {$"Некорректно введены границы цен: {errorMessage}"});
					}
					prev = markup;
				}
			}
			var ranges = source.Where(m => m.Type == MarkupType.VitallyImportant).Select(m => m.Begin);
			if (ranges.Intersect(new decimal[] {0, 50, 500}).Count() < 3)
				errors.Add(new[] {
					"Не заданы обязательные интервалы границ цен: [0, 50], [50, 500], [500, 1000000]."
				});

			if (errors.Count == 0) {
				errors = null;
			}

			return errors;
		}

		public string GetLogState()
		{
			return $"Границы: {Math.Round(Begin, 2)} - {Math.Round(End, 2)},<br/> нац.: {Math.Round(Markup, 2)}%,<br/> макс.нац: {Math.Round(MaxMarkup, 2)}%,<br/>  макс.нац. опт.: {Math.Round(MaxSupplierMarkup, 2)}% ";
		}
	}
}