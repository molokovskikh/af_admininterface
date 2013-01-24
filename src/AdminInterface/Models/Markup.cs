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
}