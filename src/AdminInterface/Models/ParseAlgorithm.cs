﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Castle.ActiveRecord;

namespace AdminInterface.Models
{
	[ActiveRecord("ParseAlgorithm", Schema = "ordersendrules", Lazy = true)]
	public class ParseAlgorithm
	{
		public ParseAlgorithm()
		{
		}

		public ParseAlgorithm(string name)
		{
			Name = name;
		}

		[PrimaryKey]
		public virtual uint Id { get; set; }

		[Property]
		public virtual string Name { get; set; }
	}
}