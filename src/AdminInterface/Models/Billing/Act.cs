using System;
using System.Collections.Generic;
using System.Linq;
using AdminInterface.Controllers;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.Components.Validator;

namespace AdminInterface.Models.Billing
{
	[ActiveRecord(Schema = "Billing")]
	public class Act : ActiveRecordLinqBase<Act>
	{
		public Act()
		{}

		public Act(DateTime actDate, params Invoice[] invoices)
		{
			Period = invoices.Select(i => i.Period).Distinct().Single();
			Payer = invoices.Select(i => i.Payer).Distinct().Single();
			Recipient = invoices.Select(i => i.Recipient).Distinct().Single();
			PayerName = invoices.Select(i => i.PayerName).Distinct().Single();
			Customer = invoices.Select(i => i.Customer).Distinct().Single();

			ActDate = Payer.GetDocumentDate(actDate);
			var invoiceParts = invoices.SelectMany(i => i.Parts);
			if (Payer.InvoiceSettings.DoNotGroupParts)
			{
				Parts = invoiceParts
					.Select(p => new ActPart(p.Name, p.Count, p.Cost))
					.ToList();
			}
			else
			{
				Parts = invoiceParts
					.GroupBy(p => new {p.Name, p.Cost})
					.Select(g => new ActPart(g.Key.Name, g.Sum(i => i.Count), g.Key.Cost))
					.ToList();
			}
			CalculateSum();

			foreach(var part in invoiceParts.Where(p => p.Ad != null))
				part.Ad.Act = this;

			foreach (var invoice in invoices)
				invoice.Act = this;
		}
		
		[PrimaryKey]
		public uint Id { get; set; }

		[Property]
		public Period Period { get; set; }

		[Property]
		public DateTime ActDate { get; set; }

		[Property]
		public decimal Sum { get; set; }

		[BelongsTo]
		public Recipient Recipient { get; set; }

		[BelongsTo]
		public Payer Payer { get; set; }

		[Property]
		public string PayerName { get; set; }

		[Property]
		public string Customer { get; set; }

		[HasMany(Lazy = true)]
		public IList<Invoice> Invoices { get; set; }

		[HasMany(Cascade = ManyRelationCascadeEnum.All, Lazy = true)]
		public IList<ActPart> Parts { get; set; }

		public bool IsDuplicateDocument()
		{
			return Queryable.FirstOrDefault(a => a.Period == Period && a.Payer == Payer) != null;
		}

		public static IEnumerable<Act> Build(List<Invoice> invoices, DateTime documentDate)
		{
			return invoices
				.Where(i => i.Act == null)
				.GroupBy(i => new { i.Payer, i.PayerName, i.Customer, i.Recipient })
				.Select(g => new Act(documentDate, g.ToArray()))
				.ToList();
		}

		public void CalculateSum()
		{
			Sum = Parts.Sum(p => p.Sum);
		}
	}

	[ActiveRecord(Schema = "Billing")]
	public class ActPart
	{
		public ActPart()
		{}

		public ActPart(string name, int count, decimal cost)
		{
			Name = name;
			Count = count;
			Cost = cost;
		}

		[PrimaryKey]
		public uint Id { get; set; }

		[Property, ValidateNonEmpty]
		public string Name { get; set; }

		[Property, ValidateGreaterThanZero]
		public decimal Cost { get; set; }

		[Property, ValidateGreaterThanZero]
		public int Count { get; set; }

		public decimal Sum
		{
			get { return Count * Cost; }
		}

		[BelongsTo]
		public Act Act { get; set; }
	}
}