using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AdminInterface.Controllers;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.ActiveRecord.Linq;

namespace AdminInterface.Models.Billing
{
	[ActiveRecord(Schema = "Billing")]
	public class Act : ActiveRecordLinqBase<Act>
	{
		public Act()
		{}

		public Act(DateTime actDate, params Invoice[] invoices)
		{
			ActDate = actDate;
			Period = invoices.Select(i => i.Period).Distinct().Single();
			Payer = invoices.Select(i => i.Payer).Distinct().Single();
			Recipient = invoices.Select(i => i.Recipient).Distinct().Single();
			Parts = invoices
				.SelectMany(i => i.Parts)
				.GroupBy(p => new {p.Name, p.Cost})
				.Select(g => new ActPart(g.Key.Name, g.Sum(i => i.Count), g.Key.Cost))
				.ToList();
			Sum = Parts.Sum(p => p.Count * p.Cost);
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

		[HasMany(Cascade = ManyRelationCascadeEnum.All, Lazy = true)]
		public IList<ActPart> Parts { get; set; }

		public bool IsDuplicateDocument()
		{
			return Queryable.FirstOrDefault(a => a.Period == Period && a.Payer == Payer) != null;
		}

		public static IEnumerable<Act> Build(List<Invoice> invoices, DateTime documentDate)
		{
			return invoices
				.GroupBy(i => i.Payer)
				.Select(g => new Act(documentDate, g.ToArray()))
				.Where(a => !a.IsDuplicateDocument())
				.ToList();
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

		[Property]
		public string Name { get; set; }

		[Property]
		public decimal Cost { get; set; }

		[Property]
		public int Count { get; set; }

		public decimal Sum
		{
			get { return Count * Cost; }
		}

		[BelongsTo]
		public Act Act { get; set; }


	}
}