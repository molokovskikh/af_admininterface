using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Castle.ActiveRecord;
using Common.Web.Ui.Models;

namespace AdminInterface.Models
{
	[ActiveRecord("Markups", Schema = "Reports")]
	public class Markup
	{
		public Markup()
		{
		}

		[PrimaryKey]
		public virtual uint Id { get; set; }

		[Property]
		public virtual int Type { get; set; }

		[Property]
		public virtual decimal Begin { get; set; }

		[Property]
		public virtual decimal End { get; set; }

		[Property]
		public virtual ulong RegionId { get; set; }

		[Property]
		public virtual decimal Value { get; set; }
	}

	public class MarkupLimits
	{
		public class Limits
		{
			public decimal Begin;
			public decimal End;
			public Limits(decimal begin, decimal end)
			{
				Begin = begin;
				End = end;
			}
		}
		static readonly Limits limit1 = new Limits(0, 50);
		static readonly Limits limit2 = new Limits(50, 500);
		static readonly Limits limit3 = new Limits(500, 1000000);
		public static Limits[] markupLimits = new Limits[] { limit1, limit2, limit3 };
	}
}